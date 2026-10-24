using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DTOs;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Services;

public interface IUgyfelService
{
    Task<IEnumerable<Ugyfel>> Lista(CancellationToken ct, string? kereses = null, bool? aktiv = null);
    Task<Ugyfel> Egy(int id, CancellationToken ct);
    Task<Ugyfel> Uj(UgyfelRequest r, CancellationToken ct);
    Task<Ugyfel> Modosit(int id, UgyfelRequest r, CancellationToken ct);
}

public sealed class UgyfelService(AppDbContext db, IHttpContextAccessor accessor)
    : SzervizServiceBase(db, accessor), IUgyfelService
{
    public async Task<IEnumerable<Ugyfel>> Lista(CancellationToken ct, string? kereses = null, bool? aktiv = null)
    {
        var q = Db.Ugyfelek.AsNoTracking().AsQueryable();
        if (Ugyfel) q = q.Where(x => x.Id == UgyfelId);
        if (aktiv.HasValue) q = q.Where(x => x.Aktiv == aktiv);
        if (!string.IsNullOrWhiteSpace(kereses)) {
            var k = kereses.Trim();
            q = q.Where(x => x.Nev.Contains(k) || x.Telefon.Contains(k) || (x.Email != null && x.Email.Contains(k)));
        }
        return await q.OrderBy(x => x.Nev).ToListAsync(ct);
    }

    public async Task<Ugyfel> Egy(int id, CancellationToken ct)
    {
        var u = await Db.Ugyfelek.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return u is null || (Ugyfel && id != UgyfelId) ? throw new SzervizKivetel(404, "Az adat nem található.") : u;
    }

    public async Task<Ugyfel> Uj(UgyfelRequest r, CancellationToken ct)
    {
        var u = new Ugyfel(); Kitolt(u, r);
        Db.Ugyfelek.Add(u); await Db.SaveChangesAsync(ct);
        return u;
    }

    public async Task<Ugyfel> Modosit(int id, UgyfelRequest r, CancellationToken ct)
    {
        var u = await Db.Ugyfelek.FindAsync([id], ct);
        if (u is null) throw new SzervizKivetel(404, "Az adat nem található.");
        if (!r.Aktiv && await Db.Javitasok.AnyAsync(x => x.UgyfelId == id && x.Allapot != JavitasAllapot.Visszaadva, ct))
            throw new SzervizKivetel(409, "Vissza nem adott eszközhöz tartozó ügyfél nem kapcsolható ki.");
        Kitolt(u, r); await Db.SaveChangesAsync(ct); return u;
    }

    private static void Kitolt(Ugyfel u, UgyfelRequest r)
    {
        u.Nev = r.Nev.Trim(); u.Telefon = r.Telefon.Trim(); u.Email = Szoveg(r.Email);
        u.Cim = Szoveg(r.Cim); u.Aktiv = r.Aktiv;
    }
}
