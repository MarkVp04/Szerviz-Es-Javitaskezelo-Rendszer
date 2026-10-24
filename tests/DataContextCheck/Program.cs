using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

// Egyedi ideiglenes fájl; a felhasználó szerviz.db adatbázisát nem érinti.
var path = Path.Combine(Path.GetTempPath(), $"szerviz-context-{Guid.NewGuid():N}.db");
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={path};Pooling=False").Options;
var passed = 0;
void Check(string name, bool condition)
{
    if (!condition) throw new Exception(name);
    Console.WriteLine($"PASS {name}"); passed++;
}

try
{
    int repairId, userId;
    await using (var db = new AppDbContext(options))
    {
        await db.Database.MigrateAsync();
        var user = new Felhasznalo { Felhasznalonev = "teszt", NormalizaltFelhasznalonev = "TESZT",
            Nev = "Teszt", JelszoHash = "Kizarolag-adatbazisteszt-nincs-belepesi-fiok", JogosultsagId = 3 };
        var customer = new Ugyfel { Nev = "Teszt ügyfél", Telefon = "12345" };
        var device = new Eszkoz { Tulajdonos = customer, EszkozTipus = "Laptop" };
        var repair = new Javitas { Ugyfel = customer, Eszkoz = device, BejelentettHiba = "Teszt" };
        db.Add(user); db.Add(repair); await db.SaveChangesAsync();
        repairId = repair.Id; userId = user.Id;
    }

    await using (var first = new AppDbContext(options))
    await using (var second = new AppDbContext(options))
    {
        var a = await first.Javitasok.SingleAsync(x => x.Id == repairId);
        var b = await second.Javitasok.SingleAsync(x => x.Id == repairId);
        a.Megjegyzes = "Első módosítás"; a.Verzio++;
        b.Megjegyzes = "Elavult második módosítás"; b.Verzio++;
        second.JavitasKoltsegek.Add(new JavitasKoltseg { JavitasId = repairId,
            RogzitoFelhasznaloId = userId, Megnevezes = "Ütköző költség", Osszeg = 999, Tipus = KoltsegTipus.Egyeb });
        await first.SaveChangesAsync();
        bool conflict = false;
        try { await second.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { conflict = true; }
        Check("Párhuzamos javításmódosítás ütközést jelez", conflict);
    }

    await using (var db = new AppDbContext(options))
    {
        var repair = await db.Javitasok.AsNoTracking().SingleAsync(x => x.Id == repairId);
        Check("Az első módosítás marad meg", repair.Megjegyzes == "Első módosítás" && repair.Verzio == 2);
        Check("Ütközéskor a kapcsolódó költség sem mentődik el", !await db.JavitasKoltsegek.AnyAsync());
        Check("SQLite-ból UTC időpont olvasható vissza", repair.AtvetelDatuma.Kind == DateTimeKind.Utc);
        Check("A pilot állapotkódjai változatlanok", (int)JavitasAllapot.Atvett == 1 && (int)JavitasAllapot.Lezart == 2);
        await db.GetService<IMigrator>().MigrateAsync("Initial");
        await using var command = db.Database.GetDbConnection().CreateCommand();
        await db.Database.OpenConnectionAsync();
        command.CommandText = "SELECT COUNT(*) FROM Javitasok WHERE Id = " + repairId;
        Check("Visszaállításkor a pilot javításai megmaradnak", Convert.ToInt32(await command.ExecuteScalarAsync()) == 1);
        await db.Database.CloseConnectionAsync();
        await db.Database.MigrateAsync();
        Check("Visszaállítás után újra alkalmazható a bővítés", await db.Javitasok.CountAsync() == 1);
    }
    Console.WriteLine($"{passed} adatbázis-ellenőrzés sikeres.");
}
finally
{
    // Csak az itt létrehozott, pontosan ismert nevű ideiglenes fájlok törlődnek.
    foreach (var suffix in new[] { "", "-wal", "-shm" })
        if (File.Exists(path + suffix)) File.Delete(path + suffix);
}
