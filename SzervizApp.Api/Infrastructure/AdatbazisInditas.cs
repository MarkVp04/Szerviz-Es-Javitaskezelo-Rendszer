using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Infrastructure;

public static class AdatbazisInditas
{
    public static async Task Inicializalas(WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Felhasznalo>>();
        await db.Database.MigrateAsync();
        if (!await db.Felhasznalok.AnyAsync())
        {
            var jelszo = app.Configuration["Bootstrap:AdminJelszo"];
            if (string.IsNullOrWhiteSpace(jelszo) || jelszo.Length < 10 || jelszo.Length > 128)
                throw new InvalidOperationException("Az első indításhoz állítsd be a Bootstrap:AdminJelszo értékét (10–128 karakter). Fejlesztéshez használd a Development környezetet.");
            var nev = app.Configuration["Bootstrap:AdminNev"] ?? "manager";
            if (string.IsNullOrWhiteSpace(nev) || nev.Trim().Length > 80)
                throw new InvalidOperationException("Érvénytelen Bootstrap:AdminNev.");
            var manager = new Felhasznalo {
                Felhasznalonev = nev.Trim(), NormalizaltFelhasznalonev = nev.Trim().ToUpperInvariant(),
                Nev = "Szervizvezető", JogosultsagId = 3
            };
            manager.JelszoHash = hasher.HashPassword(manager, jelszo);
            db.Felhasznalok.Add(manager);
            await db.SaveChangesAsync();
        }
        if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("DemoAdatok:Engedelyezve"))
            await TesztAdatok.Feltoltes(db, hasher, app.Configuration, app.Logger);
    }
}
