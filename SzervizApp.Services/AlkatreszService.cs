using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DTOs;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Services;

public interface IAlkatreszService
{
    Task<IEnumerable<Alkatresz>> Lista(CancellationToken ct, string? kereses = null, bool? aktiv = null);
    Task<Alkatresz> Egy(int id, CancellationToken ct);
    Task<Alkatresz> Uj(AlkatreszRequest r, CancellationToken ct);
    Task<Alkatresz> Modosit(int id, AlkatreszRequest r, CancellationToken ct);
}

public sealed class AlkatreszService(AppDbContext db, IHttpContextAccessor accessor)
    : SzervizServiceBase(db, accessor), IAlkatreszService
{
    public async Task<IEnumerable<Alkatresz>> Lista(CancellationToken ct, string? kereses = null, bool? aktiv = null)
    {
        var q = Db.Alkatreszek.AsNoTracking().AsQueryable();
        if (aktiv.HasValue) q = q.Where(x => x.Aktiv == aktiv);
        if (!string.IsNullOrWhiteSpace(kereses)) {
            var k = kereses.Trim(); q = q.Where(x => x.Nev.Contains(k) || x.Cikkszam.Contains(k));
        }
        return await q.OrderBy(x => x.Nev).ToListAsync(ct);
    }

    public async Task<Alkatresz> Egy(int id, CancellationToken ct)
    {
        var a = await Db.Alkatreszek.FindAsync([id], ct); return a is null ? throw new SzervizKivetel(404, "Az adat nem található.") : a;
    }

    public async Task<Alkatresz> Uj(AlkatreszRequest r, CancellationToken ct)
    {
        var kod = r.Cikkszam.Trim().ToUpperInvariant();
        if (await Db.Alkatreszek.AnyAsync(x => x.Cikkszam == kod, ct)) throw new SzervizKivetel(409, "Foglalt cikkszám.");
        var a = new Alkatresz(); Kitolt(a, r); Db.Alkatreszek.Add(a); await Db.SaveChangesAsync(ct);
        return a;
    }

    public async Task<Alkatresz> Modosit(int id, AlkatreszRequest r, CancellationToken ct)
    {
        var a = await Db.Alkatreszek.FindAsync([id], ct); if (a is null) throw new SzervizKivetel(404, "Az adat nem található.");
        var kod = r.Cikkszam.Trim().ToUpperInvariant();
        if (await Db.Alkatreszek.AnyAsync(x => x.Id != id && x.Cikkszam == kod, ct)) throw new SzervizKivetel(409, "Foglalt cikkszám.");
        if (a.Egyseg != r.Egyseg.Trim() && await Db.AlkatreszFelhasznalasok.AnyAsync(x => x.AlkatreszId == id, ct))
            throw new SzervizKivetel(409, "Már felhasznált alkatrész mértékegysége nem változtatható meg. Eltérő egységhez vegyél fel új cikkszámot.");
        Kitolt(a, r); await Db.SaveChangesAsync(ct); return a;
    }

    private static void Kitolt(Alkatresz a, AlkatreszRequest r)
    {
        a.Cikkszam = r.Cikkszam.Trim().ToUpperInvariant(); a.Nev = r.Nev.Trim(); a.Gyarto = Szoveg(r.Gyarto);
        a.Egyseg = r.Egyseg.Trim(); a.AktualisEgysegar = decimal.Round(r.AktualisEgysegar, 2); a.Aktiv = r.Aktiv;
    }
}
