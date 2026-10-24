using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DTOs;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Services;

public interface IEszkozService
{
    Task<IEnumerable<EszkozListaDto>> Lista(CancellationToken ct, string? kereses = null, int? tulajdonosId = null);
    Task<Eszkoz> Egy(int id, CancellationToken ct);
    Task<IEnumerable<JavitasDto>> Elozmenyek(int id, CancellationToken ct);
    Task<Eszkoz> Uj(EszkozRequest r, CancellationToken ct);
    Task<Eszkoz> Modosit(int id, EszkozRequest r, CancellationToken ct);
}

public sealed class EszkozService(AppDbContext db, IHttpContextAccessor accessor)
    : SzervizServiceBase(db, accessor), IEszkozService
{
    public async Task<IEnumerable<EszkozListaDto>> Lista(CancellationToken ct, string? kereses = null, int? tulajdonosId = null)
    {
        var q = Db.Eszkozok.AsNoTracking().AsQueryable();
        if (Ugyfel) q = q.Where(x => x.TulajdonosId == UgyfelId);
        if (tulajdonosId.HasValue) q = q.Where(x => x.TulajdonosId == tulajdonosId);
        if (!string.IsNullOrWhiteSpace(kereses)) {
            var k = kereses.Trim();
            q = q.Where(x => x.EszkozTipus.Contains(k) || (x.Gyarto != null && x.Gyarto.Contains(k))
                || (x.Sorozatszam != null && x.Sorozatszam.Contains(k)));
        }
        return await q.OrderBy(x => x.Id).Select(x => new EszkozListaDto(x.Id, x.TulajdonosId, x.EszkozTipus,
            x.Gyarto, x.Sorozatszam, x.VasarlasDatuma, x.Megjegyzes, x.Letrehozva)).ToListAsync(ct);
    }

    public async Task<Eszkoz> Egy(int id, CancellationToken ct)
    {
        var e = await Db.Eszkozok.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return e is null || (Ugyfel && e.TulajdonosId != UgyfelId) ? throw new SzervizKivetel(404, "Az adat nem található.") : e;
    }

    public async Task<IEnumerable<JavitasDto>> Elozmenyek(int id, CancellationToken ct)
    {
        var e = await Db.Eszkozok.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (e is null || (Ugyfel && e.TulajdonosId != UgyfelId && !await LathatoJavitasok.AnyAsync(x => x.EszkozId == id, ct)))
            throw new SzervizKivetel(404, "Az adat nem található.");
        // Tulajdonosváltás után sem látható más ügyfél javítása.
        return (await LathatoJavitasok.AsNoTracking().Where(x => x.EszkozId == id)
            .OrderByDescending(x => x.AtvetelDatuma).ToListAsync(ct)).Select(ToDto);
    }

    public async Task<Eszkoz> Uj(EszkozRequest r, CancellationToken ct)
    {
        await EllenorizAdatokat(r, ct);
        var e = new Eszkoz(); Kitolt(e, r); Db.Eszkozok.Add(e); await Db.SaveChangesAsync(ct);
        return e;
    }

    public async Task<Eszkoz> Modosit(int id, EszkozRequest r, CancellationToken ct)
    {
        var e = await Db.Eszkozok.FindAsync([id], ct); if (e is null) throw new SzervizKivetel(404, "Az adat nem található.");
        await EllenorizAdatokat(r, ct);
        if (e.TulajdonosId != r.TulajdonosId && await Db.Javitasok.AnyAsync(x => x.EszkozId == id && x.Allapot != JavitasAllapot.Visszaadva, ct))
            throw new SzervizKivetel(409, "Tulajdonos csak az eszköz visszaadása után módosítható.");
        Kitolt(e, r); await Db.SaveChangesAsync(ct); return e;
    }

    private async Task EllenorizAdatokat(EszkozRequest r, CancellationToken ct)
    {
        if (!await Db.Ugyfelek.AnyAsync(x => x.Id == r.TulajdonosId && x.Aktiv, ct))
            throw new SzervizKivetel(400, "A tulajdonos nem létező vagy inaktív ügyfél.");
        if (r.VasarlasDatuma > DateTimeOffset.UtcNow) throw new SzervizKivetel(400, "A vásárlás nem lehet jövőbeli.");
        return;
    }

    private static void Kitolt(Eszkoz e, EszkozRequest r)
    {
        e.TulajdonosId = r.TulajdonosId; e.EszkozTipus = r.EszkozTipus.Trim(); e.Gyarto = Szoveg(r.Gyarto);
        e.Sorozatszam = Szoveg(r.Sorozatszam); e.Megjegyzes = Szoveg(r.Megjegyzes); e.VasarlasDatuma = r.VasarlasDatuma?.UtcDateTime;
    }
}
