using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DTOs;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Services;

public interface IBevizsgalasService
{
    Task<BevizsgalasDto> Egy(int javitasId, CancellationToken ct);
    Task<BevizsgalasDto> Uj(int javitasId, BevizsgalasRequest r, CancellationToken ct);
    Task<BevizsgalasDto> Modosit(int javitasId, BevizsgalasRequest r, CancellationToken ct);
}

public sealed class BevizsgalasService(AppDbContext db, IHttpContextAccessor accessor)
    : SzervizServiceBase(db, accessor), IBevizsgalasService
{
    public async Task<BevizsgalasDto> Egy(int javitasId, CancellationToken ct)
    {
        var j = await LathatoJavitasok.AsNoTracking().SingleOrDefaultAsync(x => x.Id == javitasId, ct);
        if (j is null) throw new SzervizKivetel(404, "Az adat nem található.");
        var b = await Db.Bevizsgalasok.AsNoTracking().SingleOrDefaultAsync(x => x.JavitasId == javitasId, ct);
        return b is null ? throw new SzervizKivetel(404, "Az adat nem található.") : Adatok(b);
    }

    public async Task<BevizsgalasDto> Uj(int javitasId, BevizsgalasRequest r, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([javitasId], ct);
        Ellenoriz(j, true);
        if (!j!.FelelosFelhasznaloId.HasValue) throw new SzervizKivetel(409, "Előbb jelölj ki felelőst.");
        if (await Db.Bevizsgalasok.AnyAsync(x => x.JavitasId == javitasId, ct))
            throw new SzervizKivetel(409, "Már van bevizsgálás; a PUT végponttal módosítható.");
        var b = new Bevizsgalas { JavitasId = javitasId, Eredmeny = r.Eredmeny.Trim(),
            BecsultKoltseg = r.BecsultKoltseg.HasValue ? decimal.Round(r.BecsultKoltseg.Value, 2) : null,
            VegezteFelhasznaloId = FelhasznaloId };
        Db.Bevizsgalasok.Add(b);
        if (j.Allapot == JavitasAllapot.Atvett) j.Allapot = JavitasAllapot.BevizsgalasAlatt;
        Valtozott(j); await Db.SaveChangesAsync(ct);
        return Adatok(b);
    }

    public async Task<BevizsgalasDto> Modosit(int javitasId, BevizsgalasRequest r, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([javitasId], ct);
        Ellenoriz(j, true);
        var b = await Db.Bevizsgalasok.SingleOrDefaultAsync(x => x.JavitasId == javitasId, ct);
        if (b is null) throw new SzervizKivetel(404, "Az adat nem található.");
        b.Eredmeny = r.Eredmeny.Trim(); b.BecsultKoltseg = r.BecsultKoltseg.HasValue ? decimal.Round(r.BecsultKoltseg.Value, 2) : null;
        b.VegezteFelhasznaloId = FelhasznaloId; b.Datum = DateTime.UtcNow;
        Valtozott(j!); await Db.SaveChangesAsync(ct); return Adatok(b);
    }

    private static BevizsgalasDto Adatok(Bevizsgalas b) => new(b.Id, b.JavitasId, b.Eredmeny, b.BecsultKoltseg, b.Datum, b.VegezteFelhasznaloId);
}
