using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DTOs;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Services;

public interface IJogosultsagService
{
    Task<IEnumerable<Jogosultsag>> Lista(CancellationToken ct);
    Task<Jogosultsag> Modosit(int id, JogosultsagRequest r, CancellationToken ct);
}

public sealed class JogosultsagService(AppDbContext db, IHttpContextAccessor accessor)
    : SzervizServiceBase(db, accessor), IJogosultsagService
{
    public async Task<IEnumerable<Jogosultsag>> Lista(CancellationToken ct) => await Db.Jogosultsagok.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);

    public async Task<Jogosultsag> Modosit(int id, JogosultsagRequest r, CancellationToken ct)
    {
        var j = await Db.Jogosultsagok.FindAsync([id], ct);
        if (j is null) throw new SzervizKivetel(404, "Az adat nem található.");
        // A kód rögzített: a műveletekhez tartozó jogokat a controller attribútumok határozzák meg.
        j.Nev = r.Nev.Trim(); j.Leiras = Szoveg(r.Leiras);
        await Db.SaveChangesAsync(ct);
        return j;
    }
}
