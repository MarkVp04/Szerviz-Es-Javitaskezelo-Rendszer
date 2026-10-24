using System.Text.Json.Serialization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SzervizApp.Api.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SzervizApp.Services;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SzervizApp API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        In = ParameterLocation.Header, Name = "Authorization", Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer", BearerFormat = "JWT",
        Description = "Először POST /api/auth/bejelentkezes. A kapott tokennel ide írd: Bearer {token}"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
    options.OperationFilter<JogosultsagDokumentacio>();
});
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(
    builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=szerviz.db"));
builder.Services.AddScoped<IPasswordHasher<Felhasznalo>, PasswordHasher<Felhasznalo>>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFelhasznaloService, FelhasznaloService>();
builder.Services.AddScoped<IJogosultsagService, JogosultsagService>();
builder.Services.AddScoped<IUgyfelService, UgyfelService>();
builder.Services.AddScoped<IEszkozService, EszkozService>();
builder.Services.AddScoped<IJavitasService, JavitasService>();
builder.Services.AddScoped<IBevizsgalasService, BevizsgalasService>();
builder.Services.AddScoped<IMunkanaploService, MunkanaploService>();
builder.Services.AddScoped<IAlkatreszService, AlkatreszService>();
builder.Services.AddScoped<IAlkatreszFelhasznalasService, AlkatreszFelhasznalasService>();
builder.Services.AddScoped<IKoltsegService, KoltsegService>();
builder.Services.AddScoped<IUzenetService, UzenetService>();

var jwt = new JwtBeallitasok(builder.Configuration);
builder.Services.AddSingleton(jwt);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer, ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(jwt.SecretKey),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256], ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.Name, RoleClaimType = ClaimTypes.Role
    };
    options.Events = new JwtBearerEvents {
        OnTokenValidated = async context => {
            var auth = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
            if (!await auth.TokenErvenyes(context.Principal!, context.HttpContext.RequestAborted))
                context.Fail("A fiók vagy a token már nem érvényes.");
        }
    };
});
builder.Services.AddAuthorization(options => options.FallbackPolicy =
    new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
// Egy mentésbe kerül a részadat és a javítás verziója; ütközéskor az egész visszagördül.
app.Use(async (context, next) => {
    try { await next(); }
    catch (SzervizKivetel ex) {
        context.Response.StatusCode = ex.Kod;
        await context.Response.WriteAsJsonAsync(new { message = ex.Message });
    }
    catch (DbUpdateConcurrencyException) {
        context.Response.StatusCode = 409;
        await context.Response.WriteAsJsonAsync(new { message = "A javítás időközben megváltozott. Töltsd újra az adatokat." });
    }
    catch (DbUpdateException ex) {
        app.Logger.LogWarning(ex, "Az adatbázis nem fogadta el a módosítást.");
        context.Response.StatusCode = 409;
        await context.Response.WriteAsJsonAsync(new { message = "Ütköző vagy érvénytelen adatkapcsolat. Ellenőrizd az azonosítókat és az egyedi adatokat." });
    }
});
await AdatbazisInditas.Inicializalas(app);
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
