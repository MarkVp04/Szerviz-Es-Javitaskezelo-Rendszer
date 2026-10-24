using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SzervizApp.DTOs;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Services;

public interface IAuthService
{
    Task<LoginResponseDto> Belepes(BelepesRequest request, CancellationToken ct);
    Task Kilepes(CancellationToken ct);
    Task<FelhasznaloDto> En(CancellationToken ct);
    Task<LoginResponseDto> Jelszo(JelszoRequest request, CancellationToken ct);
    Task<bool> TokenErvenyes(ClaimsPrincipal principal, CancellationToken ct);
}

public sealed class AuthService(AppDbContext db, IHttpContextAccessor accessor,
    IPasswordHasher<Felhasznalo> hasher, JwtBeallitasok jwt) : SzervizServiceBase(db, accessor), IAuthService
{
    public async Task<LoginResponseDto> Belepes(BelepesRequest request, CancellationToken ct)
    {
        var nev = request.Felhasznalonev.Trim().ToUpperInvariant();
        var f = await Db.Felhasznalok.Include(x => x.Jogosultsag).Include(x => x.Ugyfel)
            .SingleOrDefaultAsync(x => x.NormalizaltFelhasznalonev == nev, ct);
        if (f is null || !f.Aktiv || (f.Jogosultsag.Kod == Szerepkorok.Ugyfel && f.Ugyfel?.Aktiv != true))
            throw new SzervizKivetel(401, "Hibás felhasználónév vagy jelszó.");
        var eredmeny = hasher.VerifyHashedPassword(f, f.JelszoHash, request.Jelszo);
        if (eredmeny == PasswordVerificationResult.Failed)
            throw new SzervizKivetel(401, "Hibás felhasználónév vagy jelszó.");
        if (eredmeny == PasswordVerificationResult.SuccessRehashNeeded)
        {
            f.JelszoHash = hasher.HashPassword(f, request.Jelszo);
            await Db.SaveChangesAsync(ct);
        }
        return GenerateToken(f);
    }

    public async Task Kilepes(CancellationToken ct)
    {
        var f = await Db.Felhasznalok.SingleAsync(x => x.Id == FelhasznaloId, ct);
        // A meglévő belépési verzióval az eddig kiadott tokenek azonnal visszavonhatók.
        f.BelepesVerzio = Guid.NewGuid();
        await Db.SaveChangesAsync(ct);
    }

    public async Task<FelhasznaloDto> En(CancellationToken ct) => FelhasznaloAdatok.ToDto(await Db.Felhasznalok
        .AsNoTracking().Include(x => x.Jogosultsag).SingleAsync(x => x.Id == FelhasznaloId, ct));

    public async Task<LoginResponseDto> Jelszo(JelszoRequest request, CancellationToken ct)
    {
        var f = await Db.Felhasznalok.Include(x => x.Jogosultsag).SingleAsync(x => x.Id == FelhasznaloId, ct);
        if (hasher.VerifyHashedPassword(f, f.JelszoHash, request.RegiJelszo) == PasswordVerificationResult.Failed)
            throw new SzervizKivetel(400, "A régi jelszó hibás.");
        if (string.IsNullOrWhiteSpace(request.UjJelszo)) throw new SzervizKivetel(400, "Hibás új jelszó.");
        f.JelszoHash = hasher.HashPassword(f, request.UjJelszo);
        f.BelepesVerzio = Guid.NewGuid();
        await Db.SaveChangesAsync(ct);
        return GenerateToken(f);
    }

    public async Task<bool> TokenErvenyes(ClaimsPrincipal principal, CancellationToken ct)
    {
        if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return false;
        var f = await Db.Felhasznalok.AsNoTracking().Include(x => x.Jogosultsag).Include(x => x.Ugyfel)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        return f is not null && f.Aktiv
            && f.BelepesVerzio.ToString() == principal.FindFirstValue("belepesVerzio")
            && f.Jogosultsag.Kod == principal.FindFirstValue(ClaimTypes.Role)
            && (f.UgyfelId?.ToString() ?? "") == principal.FindFirstValue("ugyfelId")
            && (f.Jogosultsag.Kod != Szerepkorok.Ugyfel || f.Ugyfel?.Aktiv == true);
    }

    private LoginResponseDto GenerateToken(Felhasznalo f)
    {
        var most = DateTime.UtcNow;
        var lejarat = most.AddDays(jwt.ExpiresInDays);
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, f.Id.ToString()),
            new Claim(ClaimTypes.Name, f.Felhasznalonev),
            new Claim(ClaimTypes.Role, f.Jogosultsag.Kod),
            new Claim("ugyfelId", f.UgyfelId?.ToString() ?? ""),
            new Claim("belepesVerzio", f.BelepesVerzio.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(issuer: jwt.Issuer, audience: jwt.Audience,
            claims: claims, notBefore: most, expires: lejarat,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(jwt.SecretKey), SecurityAlgorithms.HmacSha256));
        return new LoginResponseDto(new JwtSecurityTokenHandler().WriteToken(token), lejarat,
            f.Id, f.Felhasznalonev, f.Nev, f.Email, f.Jogosultsag.Kod, f.UgyfelId);
    }
}
