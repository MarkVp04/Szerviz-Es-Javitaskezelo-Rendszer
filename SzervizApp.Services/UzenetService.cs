using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DTOs;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Services;

public interface IUzenetService
{
    Task<IEnumerable<UzenetListaDto>> Lista(CancellationToken ct, int? ugyfelId = null, int? javitasId = null);
    Task<UzenetDto> Kuldes(UzenetRequest r, CancellationToken ct);
}

public sealed class UzenetService(AppDbContext db, IHttpContextAccessor accessor)
    : SzervizServiceBase(db, accessor), IUzenetService
{
    public async Task<IEnumerable<UzenetListaDto>> Lista(CancellationToken ct, int? ugyfelId = null, int? javitasId = null)
    {
        var q = Db.Uzenetek.AsNoTracking().AsQueryable();
        if (Ugyfel) q = q.Where(x => x.UgyfelId == UgyfelId);
        if (ugyfelId.HasValue) q = q.Where(x => x.UgyfelId == ugyfelId);
        if (javitasId.HasValue) q = q.Where(x => x.JavitasId == javitasId);
        return await q.OrderBy(x => x.KuldesDatuma).Select(x => new UzenetListaDto(x.Id, x.UgyfelId, x.JavitasId,
            x.FeladoFelhasznaloId, x.FeladoFelhasznalo.Nev, x.Tartalom, x.KuldesDatuma)).ToListAsync(ct);
    }

    public async Task<UzenetDto> Kuldes(UzenetRequest r, CancellationToken ct)
    {
        var ugyfelId = Ugyfel ? UgyfelId : r.UgyfelId;
        if (Ugyfel && r.UgyfelId.HasValue && r.UgyfelId != UgyfelId) throw new SzervizKivetel(403, "Nincs jogosultság.");
        if (!ugyfelId.HasValue || !await Db.Ugyfelek.AnyAsync(x => x.Id == ugyfelId && x.Aktiv, ct))
            throw new SzervizKivetel(400, "Aktív ügyfél megadása szükséges.");
        if (r.JavitasId.HasValue && !await Db.Javitasok.AnyAsync(x => x.Id == r.JavitasId && x.UgyfelId == ugyfelId, ct))
            throw new SzervizKivetel(404, "Az ügyfélhez tartozó javítás nem található.");
        var u = new Uzenet { UgyfelId = ugyfelId.Value, JavitasId = r.JavitasId,
            FeladoFelhasznaloId = FelhasznaloId, Tartalom = r.Tartalom.Trim() };
        Db.Uzenetek.Add(u); await Db.SaveChangesAsync(ct);
        return new UzenetDto(u.Id, u.UgyfelId, u.JavitasId, u.FeladoFelhasznaloId, u.Tartalom, u.KuldesDatuma);
    }
}
