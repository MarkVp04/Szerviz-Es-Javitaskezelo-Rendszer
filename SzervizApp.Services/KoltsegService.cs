using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DTOs;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Services;

public interface IKoltsegService
{
    Task<KoltsegOsszesitesDto> Osszesites(int javitasId, CancellationToken ct);
    Task<KoltsegDto> Uj(int javitasId, KoltsegRequest r, CancellationToken ct);
    Task<KoltsegDto> Modosit(int javitasId, int id, KoltsegRequest r, CancellationToken ct);
    Task Torol(int javitasId, int id, CancellationToken ct);
    Task<KoltsegOsszesitesDto> Veglegesites(int javitasId, CancellationToken ct);
}

public sealed class KoltsegService(AppDbContext db, IHttpContextAccessor accessor)
    : SzervizServiceBase(db, accessor), IKoltsegService
{
    public async Task<KoltsegOsszesitesDto> Osszesites(int javitasId, CancellationToken ct)
    {
        var j = await LathatoJavitasok.AsNoTracking().SingleOrDefaultAsync(x => x.Id == javitasId, ct);
        if (j is null) throw new SzervizKivetel(404, "Az adat nem található.");
        // SQLite nem támogatja a decimal SQL SUM-ot. Pontosan, decimal típussal számolunk a betöltött tételekből.
        var munkak = await Db.Munkanaplok.AsNoTracking().Where(x => x.JavitasId == javitasId).ToListAsync(ct);
        var alkatreszek = await Db.AlkatreszFelhasznalasok.AsNoTracking().Where(x => x.JavitasId == javitasId).ToListAsync(ct);
        var tetelek = await Db.JavitasKoltsegek.AsNoTracking().Where(x => x.JavitasId == javitasId).OrderBy(x => x.Id).ToListAsync(ct);
        var munkadij = munkak.Sum(x => decimal.Round(x.LedolgozottOra * x.Oradij, 2));
        var alkatreszDij = alkatreszek.Sum(x => decimal.Round(x.Mennyiseg * x.Egysegar, 2));
        var egyeb = tetelek.Sum(x => x.Osszeg);
        return new KoltsegOsszesitesDto(javitasId, "HUF", munkadij, alkatreszDij,
            egyeb, munkadij + alkatreszDij + egyeb, j.Garancialis,
            j.KoltsegekVeglegesitve, munkak.Count(x => x.Befejezes == null), tetelek.Select(Adatok));
    }

    public async Task<KoltsegDto> Uj(int javitasId, KoltsegRequest r, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([javitasId], ct);
        Ellenoriz(j, true);
        var k = new JavitasKoltseg { JavitasId = javitasId, RogzitoFelhasznaloId = FelhasznaloId };
        Kitolt(k, r); Db.JavitasKoltsegek.Add(k); Valtozott(j!, true); await Db.SaveChangesAsync(ct);
        return Adatok(k);
    }

    public async Task<KoltsegDto> Modosit(int javitasId, int id, KoltsegRequest r, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([javitasId], ct);
        Ellenoriz(j, true);
        var k = await Db.JavitasKoltsegek.SingleOrDefaultAsync(x => x.Id == id && x.JavitasId == javitasId, ct);
        if (k is null) throw new SzervizKivetel(404, "Az adat nem található.");
        Kitolt(k, r); Valtozott(j!, true); await Db.SaveChangesAsync(ct); return Adatok(k);
    }

    public async Task Torol(int javitasId, int id, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([javitasId], ct);
        Ellenoriz(j, true);
        var k = await Db.JavitasKoltsegek.SingleOrDefaultAsync(x => x.Id == id && x.JavitasId == javitasId, ct);
        if (k is null) throw new SzervizKivetel(404, "Az adat nem található.");
        Db.JavitasKoltsegek.Remove(k); Valtozott(j!, true); await Db.SaveChangesAsync(ct); return;
    }

    public async Task<KoltsegOsszesitesDto> Veglegesites(int javitasId, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([javitasId], ct);
        Ellenoriz(j, true);
        if (await Db.Munkanaplok.AnyAsync(m => m.JavitasId == javitasId && m.Befejezes == null, ct))
            throw new SzervizKivetel(409, "Előbb fejezd be a megkezdett munkákat.");
        j!.KoltsegekVeglegesitve = true; Valtozott(j); await Db.SaveChangesAsync(ct);
        return await Osszesites(javitasId, ct);
    }

    private static void Kitolt(JavitasKoltseg k, KoltsegRequest r)
    {
        k.Megnevezes = r.Megnevezes.Trim(); k.Tipus = r.Tipus; k.Osszeg = decimal.Round(r.Osszeg, 2);
    }

    private static KoltsegDto Adatok(JavitasKoltseg k) => new(k.Id, k.JavitasId, k.Megnevezes, k.Tipus, k.Osszeg, k.Datum, k.RogzitoFelhasznaloId);
}
