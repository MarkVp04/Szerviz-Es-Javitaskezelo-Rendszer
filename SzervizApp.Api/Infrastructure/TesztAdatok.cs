using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Infrastructure;

public static class TesztAdatok
{
    private const string Jeloles = "[TESZTADATOK-v1]";

    public static async Task Feltoltes(AppDbContext db, IPasswordHasher<Felhasznalo> hasher,
        IConfiguration configuration, ILogger logger)
    {
        // A jelölés a teljes, egy tranzakcióban mentett adatsort azonosítja.
        // A már megváltoztatott tesztadatokat és jelszavakat nem állítjuk vissza újraindításkor.
        await using var transaction = await db.Database.BeginTransactionAsync();
        if (await db.Javitasok.AnyAsync(j => j.Megjegyzes == Jeloles)) return;

        string[] nevek = ["demo.recepcios", "demo.szerelo1", "demo.szerelo2", "demo.ugyfel1", "demo.ugyfel2", "demo.ugyfel3"];
        var normalizaltNevek = nevek.Select(n => n.ToUpperInvariant()).ToArray();
        if (await db.Felhasznalok.AnyAsync(f => normalizaltNevek.Contains(f.NormalizaltFelhasznalonev)))
        {
            logger.LogWarning("A tesztadatok feltöltése kimaradt: valamelyik fenntartott demo. felhasználónév már foglalt. Meglévő fiókot nem írunk felül.");
            return;
        }
        var jelszo = configuration["DemoAdatok:Jelszo"];
        if (string.IsNullOrWhiteSpace(jelszo) || jelszo.Length < 10 || jelszo.Length > 128)
            throw new InvalidOperationException("A DemoAdatok:Jelszo beállítás 10–128 karakteres legyen.");

        var most = DateTime.UtcNow;
        var anna = new Ugyfel { Nev = "Minta Anna", Telefon = "+36 30 111 2233", Email = "anna.demo@example.com", Cim = "Budapest, Minta utca 1." };
        var peter = new Ugyfel { Nev = "Minta Péter", Telefon = "+36 20 222 3344", Email = "peter.demo@example.com", Cim = "Szeged, Minta utca 2." };
        var eszter = new Ugyfel { Nev = "Minta Eszter", Telefon = "+36 70 333 4455", Email = "eszter.demo@example.com", Cim = "Pécs, Minta utca 3." };
        db.Ugyfelek.AddRange(anna, peter, eszter);

        Felhasznalo Fiok(string nev, string teljesNev, int szerepkor, Ugyfel? ugyfel = null)
        {
            var f = new Felhasznalo { Felhasznalonev = nev, NormalizaltFelhasznalonev = nev.ToUpperInvariant(),
                Nev = teljesNev, JogosultsagId = szerepkor, Ugyfel = ugyfel, Email = ugyfel?.Email };
            f.JelszoHash = hasher.HashPassword(f, jelszo);
            db.Felhasznalok.Add(f);
            return f;
        }
        var recepcios = Fiok(nevek[0], "Minta Réka – recepciós", 1);
        var szerelo1 = Fiok(nevek[1], "Minta Ádám – szerelő", 2);
        var szerelo2 = Fiok(nevek[2], "Minta Bence – szerelő", 2);
        var annaFiok = Fiok(nevek[3], anna.Nev, 4, anna);
        Fiok(nevek[4], peter.Nev, 4, peter);
        Fiok(nevek[5], eszter.Nev, 4, eszter);

        Eszkoz Eszkoz(Ugyfel tulajdonos, string tipus, string gyarto, string sorozat)
        {
            var e = new Eszkoz { Tulajdonos = tulajdonos, EszkozTipus = tipus, Gyarto = gyarto,
                Sorozatszam = "DEMO-V2-" + sorozat, VasarlasDatuma = most.AddYears(-1) };
            db.Eszkozok.Add(e); return e;
        }
        var laptop = Eszkoz(anna, "Laptop", "Lenovo", "LAPTOP-001");
        var telefonAnna = Eszkoz(anna, "Mobiltelefon", "Samsung", "TEL-002");
        var telefonPeter = Eszkoz(peter, "Mobiltelefon", "Xiaomi", "TEL-003");
        var tablet = Eszkoz(peter, "Tablet", "Samsung", "TAB-004");
        var nyomtato = Eszkoz(eszter, "Nyomtató", "HP", "PRINT-005");

        Javitas Javitas(Eszkoz eszkoz, string hiba, JavitasAllapot allapot, DateTime atvetel, Felhasznalo? felelos)
        {
            var j = new Javitas { Ugyfel = eszkoz.Tulajdonos, Eszkoz = eszkoz, BejelentettHiba = hiba,
                Allapot = allapot, AtvetelDatuma = atvetel, FelvevoFelhasznalo = recepcios,
                FelelosFelhasznalo = felelos, TervezettBefejezes = felelos is null ? null : atvetel.AddDays(5), Megjegyzes = Jeloles };
            db.Javitasok.Add(j); return j;
        }
        var visszaadott = Javitas(laptop, "A háttértár hibás, a rendszer nem indul.", JavitasAllapot.Visszaadva, most.AddDays(-30), szerelo1);
        var atvett = Javitas(laptop, "A ventilátor hangos, tisztítás szükséges.", JavitasAllapot.Atvett, most.AddHours(-2), null);
        var folyamatban = Javitas(telefonAnna, "A telefon nem tölt megfelelően.", JavitasAllapot.JavitasAlatt, most.AddDays(-2), szerelo1);
        var varakozo = Javitas(telefonPeter, "A kijelző törött.", JavitasAllapot.AlkatreszreVar, most.AddDays(-3), szerelo2);
        var lezart = Javitas(tablet, "Beázott, nem kapcsol be.", JavitasAllapot.Lezart, most.AddDays(-8), szerelo2);
        var bevizsgalt = Javitas(nyomtato, "A nyomtató csíkosan nyomtat.", JavitasAllapot.BevizsgalasAlatt, most.AddDays(-1), szerelo1);
        bevizsgalt.Garancialis = true;

        Bevizsgalas Bevizsgalas(Javitas j, string eredmeny, decimal becsles) => new() {
            Javitas = j, Eredmeny = eredmeny, BecsultKoltseg = becsles,
            VegezteFelhasznalo = j.FelelosFelhasznalo!, Datum = j.AtvetelDatuma.AddHours(1)
        };
        db.Bevizsgalasok.AddRange(
            Bevizsgalas(visszaadott, "Az SSD meghibásodott, cserélni kell.", 26490),
            Bevizsgalas(folyamatban, "A töltőcsatlakozó sérült, cserére szorul.", 9000),
            Bevizsgalas(varakozo, "Kijelzőcsere szükséges, az alkatrészre várunk.", 34990),
            Bevizsgalas(lezart, "Az alaplap korrodált, a javítás nem gazdaságos.", 60000),
            Bevizsgalas(bevizsgalt, "A nyomtatófej tisztítása szükséges.", 0));

        var ssd = new Alkatresz { Cikkszam = "DEMO-SSD-500", Nev = "500 GB SSD", Gyarto = "Kingston", AktualisEgysegar = 18990 };
        var csatlakozo = new Alkatresz { Cikkszam = "DEMO-USB-C", Nev = "USB-C töltőcsatlakozó", AktualisEgysegar = 2490 };
        var kijelzo = new Alkatresz { Cikkszam = "DEMO-KIJELZO", Nev = "Telefon kijelzőmodul", AktualisEgysegar = 24990 };
        db.Alkatreszek.AddRange(ssd, csatlakozo, kijelzo);
        db.AlkatreszFelhasznalasok.AddRange(
            new AlkatreszFelhasznalas { Javitas = visszaadott, Alkatresz = ssd, Mennyiseg = 1, Egysegar = ssd.AktualisEgysegar, RogzitoFelhasznalo = szerelo1, Datum = most.AddDays(-29) },
            new AlkatreszFelhasznalas { Javitas = folyamatban, Alkatresz = csatlakozo, Mennyiseg = 1, Egysegar = csatlakozo.AktualisEgysegar, RogzitoFelhasznalo = szerelo1, Datum = most.AddHours(-1) });
        db.Munkanaplok.AddRange(
            new Munkanaplo { Javitas = visszaadott, Felhasznalo = szerelo1, Leiras = "SSD cseréje, rendszer telepítése és ellenőrzése.",
                Kezdes = most.AddDays(-29), Befejezes = most.AddDays(-29).AddHours(1), LedolgozottOra = 1, Oradij = 6000 },
            new Munkanaplo { Javitas = folyamatban, Felhasznalo = szerelo1, Leiras = "Töltőcsatlakozó cseréje és töltési teszt.",
                Kezdes = most.AddHours(-1), Befejezes = null, LedolgozottOra = 0, Oradij = 6000 });
        db.JavitasKoltsegek.AddRange(
            new JavitasKoltseg { Javitas = visszaadott, RogzitoFelhasznalo = szerelo1, Megnevezes = "Szállítás", Tipus = KoltsegTipus.Szallitas, Osszeg = 1500, Datum = most.AddDays(-29) },
            new JavitasKoltseg { Javitas = lezart, RogzitoFelhasznalo = szerelo2, Megnevezes = "Bevizsgálási díj", Tipus = KoltsegTipus.Bevizsgalas, Osszeg = 2500, Datum = most.AddDays(-7).AddHours(-1) });

        visszaadott.KoltsegekVeglegesitve = true;
        visszaadott.Eredmeny = JavitasEredmeny.Javitva;
        visszaadott.LezarasDatuma = most.AddDays(-28);
        visszaadott.LezarasIndoklasa = "SSD cserélve, az eszköz működését ellenőriztük.";
        db.Visszaadasok.Add(new Visszaadas { Javitas = visszaadott, RogzitoFelhasznalo = recepcios,
            VisszaadasDatuma = most.AddDays(-27), AtvevoNev = anna.Nev, Megjegyzes = "Az ügyfél működőképesen átvette." });
        lezart.KoltsegekVeglegesitve = true;
        lezart.Eredmeny = JavitasEredmeny.NemJavithato;
        lezart.LezarasDatuma = most.AddDays(-7);
        lezart.LezarasIndoklasa = "A javítás nem gazdaságos. Visszaadásra vár.";
        db.Uzenetek.AddRange(
            new Uzenet { Ugyfel = anna, Javitas = folyamatban, FeladoFelhasznalo = annaFiok,
                Tartalom = "Mikorra várható a telefonom javítása?", KuldesDatuma = most.AddHours(-3) },
            new Uzenet { Ugyfel = anna, Javitas = folyamatban, FeladoFelhasznalo = recepcios,
                Tartalom = "A csatlakozó cseréje folyamatban van, utána ellenőrizzük a töltést.", KuldesDatuma = most.AddMinutes(-30) });

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        logger.LogInformation("Tesztadatok létrehozva: 6 fiók, 3 ügyfél, 5 eszköz, 6 javítás, 3 alkatrész és kapcsolódó részadatok.");
    }
}
