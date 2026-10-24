using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DTOs;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Services;

public interface IAlkatreszFelhasznalasService
{
    Task<IEnumerable<AlkatreszFelhasznalasDto>> Lista(int javitasId, CancellationToken ct);
    Task<AlkatreszFelhasznalasDto> Uj(int javitasId, AlkatreszFelhasznalasRequest r, CancellationToken ct);
    Task Torol(int javitasId, int id, CancellationToken ct);
}

public sealed class AlkatreszFelhasznalasService(AppDbContext db, IHttpContextAccessor accessor)
    : SzervizServiceBase(db, accessor), IAlkatreszFelhasznalasService
{
    public async Task<IEnumerable<AlkatreszFelhasznalasDto>> Lista(int javitasId, CancellationToken ct)
    {
        if (!await LathatoJavitasok.AnyAsync(x => x.Id == javitasId, ct)) throw new SzervizKivetel(404, "Az adat nem található.");
        return (await Db.AlkatreszFelhasznalasok.AsNoTracking().Include(x => x.Alkatresz)
            .Where(x => x.JavitasId == javitasId).OrderBy(x => x.Id).ToListAsync(ct)).Select(Adatok);
    }

    public async Task<AlkatreszFelhasznalasDto> Uj(int javitasId, AlkatreszFelhasznalasRequest r, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([javitasId], ct);
        Ellenoriz(j, true);
        if (j!.Allapot != JavitasAllapot.JavitasAlatt) throw new SzervizKivetel(409, "Alkatrész felhasználása JavitasAlatt állapotban rögzíthető.");
        if (decimal.Round(r.Mennyiseg, 3) != r.Mennyiseg) throw new SzervizKivetel(400, "A mennyiség legfeljebb három tizedesjegyű lehet.");
        var a = await Db.Alkatreszek.SingleOrDefaultAsync(x => x.Id == r.AlkatreszId && x.Aktiv, ct);
        if (a is null) throw new SzervizKivetel(400, "Nem létező vagy inaktív alkatrész.");
        var f = new AlkatreszFelhasznalas { JavitasId = javitasId, AlkatreszId = a.Id,
            RogzitoFelhasznaloId = FelhasznaloId, Mennyiseg = r.Mennyiseg, Egysegar = a.AktualisEgysegar, Alkatresz = a };
        Db.AlkatreszFelhasznalasok.Add(f); Valtozott(j, true); await Db.SaveChangesAsync(ct);
        return Adatok(f);
    }

    public async Task Torol(int javitasId, int id, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([javitasId], ct);
        Ellenoriz(j, true);
        var f = await Db.AlkatreszFelhasznalasok.SingleOrDefaultAsync(x => x.Id == id && x.JavitasId == javitasId, ct);
        if (f is null) throw new SzervizKivetel(404, "Az adat nem található.");
        // Téves tétel nyitott javításnál javítható; lezárt előzményt nem törlünk.
        Db.AlkatreszFelhasznalasok.Remove(f); Valtozott(j!, true); await Db.SaveChangesAsync(ct); return;
    }

    private static AlkatreszFelhasznalasDto Adatok(AlkatreszFelhasznalas f) => new(f.Id, f.JavitasId, f.AlkatreszId,
        f.Alkatresz.Cikkszam, f.Alkatresz.Nev, f.Alkatresz.Egyseg, f.Mennyiseg, f.Egysegar,
        decimal.Round(f.Mennyiseg * f.Egysegar, 2), f.Datum, f.RogzitoFelhasznaloId);
}
