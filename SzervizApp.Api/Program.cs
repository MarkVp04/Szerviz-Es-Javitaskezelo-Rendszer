using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
//using Microsoft.OpenApi;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=szerviz.db"));

var app = builder.Build();

//*****************************************************************************
// Tesztadatok feltöltése fejlesztõi környezetben
if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();

    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    // Alkalmazzuk az esetleg még hiányzó migrációkat.
    await db.Database.MigrateAsync();

    // Csak üres ügyféltábla esetén töltjük fel a tesztadatokat.
    if (!await db.Ugyfelek.AnyAsync())
    {
        // 1. Ügyfelek
        var anna = new Ugyfel
        {
            Nev = "Kovács Anna",
            Telefon = "+36 30 111 2233",
            Email = "anna@example.com"
        };

        var peter = new Ugyfel
        {
            Nev = "Nagy Péter",
            Telefon = "+36 20 222 3344",
            Email = "peter@example.com"
        };

        var eszter = new Ugyfel
        {
            Nev = "Tóth Eszter",
            Telefon = "+36 70 333 4455",
            Email = "eszter@example.com"
        };

        db.Ugyfelek.AddRange(anna, peter, eszter);

        // 2. Eszközök
        var laptop = new Eszkoz
        {
            Tulajdonos = anna,
            EszkozTipus = "Laptop",
            Gyarto = "Lenovo",
            Sorozatszam = "DEMO-LEN-001"
        };

        var telefon = new Eszkoz
        {
            Tulajdonos = peter,
            EszkozTipus = "Mobiltelefon",
            Gyarto = "Samsung",
            Sorozatszam = "DEMO-SAM-002"
        };

        var konzol = new Eszkoz
        {
            Tulajdonos = eszter,
            EszkozTipus = "Játékkonzol",
            Gyarto = "Sony",
            Sorozatszam = "DEMO-SON-003"
        };

        db.Eszkozok.AddRange(laptop, telefon, konzol);

        // 3. Javítások
        var javitas1 = new Javitas
        {
            Ugyfel = anna,
            Eszkoz = laptop,
            BejelentettHiba = "A laptop nem kapcsol be.",
            AtvetelDatuma = DateTime.UtcNow.AddDays(-2),
            Allapot = JavitasAllapot.Atvett
        };

        var javitas2 = new Javitas
        {
            Ugyfel = peter,
            Eszkoz = telefon,
            BejelentettHiba = "A telefon nem tölt.",
            AtvetelDatuma = DateTime.UtcNow.AddDays(-5),
            Allapot = JavitasAllapot.Lezart,
            LezarasDatuma = DateTime.UtcNow.AddDays(-3),
            Eredmeny = JavitasEredmeny.Javitva,
            LezarasIndoklasa = "A töltõcsatlakozó tisztítása és javítása megtörtént."
        };

        var javitas3 = new Javitas
        {
            Ugyfel = eszter,
            Eszkoz = konzol,
            BejelentettHiba = "A konzol játék közben túlmelegszik.",
            AtvetelDatuma = DateTime.UtcNow.AddDays(-8),
            Allapot = JavitasAllapot.Lezart,
            LezarasDatuma = DateTime.UtcNow.AddDays(-6),
            Eredmeny = JavitasEredmeny.NemJavithato,
            LezarasIndoklasa = "Az alaplap sérült, a javítás nem gazdaságos."
        };

        db.Javitasok.AddRange(javitas1, javitas2, javitas3);

        // Az összes tesztadat mentése egy mûvelettel.
        await db.SaveChangesAsync();

        Console.WriteLine("A tesztadatok sikeresen bekerültek az adatbázisba.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();