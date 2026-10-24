using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DTOs;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Services;

public interface IMunkanaploService
{
    Task<IEnumerable<MunkanaploDto>> Lista(int javitasId, CancellationToken ct);
    Task<MunkanaploDto> Uj(int javitasId, MunkanaploRequest r, CancellationToken ct);
    Task<MunkanaploDto> Modosit(int javitasId, int id, MunkanaploRequest r, CancellationToken ct);
}

public sealed class MunkanaploService(AppDbContext db, IHttpContextAccessor accessor)
    : SzervizServiceBase(db, accessor), IMunkanaploService
{
    public async Task<IEnumerable<MunkanaploDto>> Lista(int javitasId, CancellationToken ct)
    {
        if (!await LathatoJavitasok.AnyAsync(j => j.Id == javitasId, ct)) throw new SzervizKivetel(404, "Az adat nem található.");
        return (await Db.Munkanaplok.AsNoTracking().Where(x => x.JavitasId == javitasId).OrderBy(x => x.Kezdes).ToListAsync(ct)).Select(Adatok);
    }

    public async Task<MunkanaploDto> Uj(int javitasId, MunkanaploRequest r, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([javitasId], ct);
        Ellenoriz(j, true);
        await MunkaEllenorzes(j!, r, ct);
        var m = new Munkanaplo { JavitasId = javitasId, FelhasznaloId = FelhasznaloId };
        Kitolt(m, r); Db.Munkanaplok.Add(m); Valtozott(j!, true);
        await Db.SaveChangesAsync(ct);
        return Adatok(m);
    }

    public async Task<MunkanaploDto> Modosit(int javitasId, int id, MunkanaploRequest r, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([javitasId], ct);
        Ellenoriz(j, true);
        var m = await Db.Munkanaplok.SingleOrDefaultAsync(x => x.Id == id && x.JavitasId == javitasId, ct);
        if (m is null) throw new SzervizKivetel(404, "Az adat nem található.");
        await MunkaEllenorzes(j!, r, ct);
        Kitolt(m, r); Valtozott(j!, true); await Db.SaveChangesAsync(ct); return Adatok(m);
    }

    private async Task MunkaEllenorzes(Javitas j, MunkanaploRequest r, CancellationToken ct)
    {
        if (j.Allapot != JavitasAllapot.JavitasAlatt)
            throw new SzervizKivetel(409, "Munkát a JavitasAlatt állapotban rögzíthetsz vagy módosíthatsz.");
        if (!await Db.Bevizsgalasok.AnyAsync(b => b.JavitasId == j.Id, ct))
            throw new SzervizKivetel(409, "Előbb bevizsgálás kell.");
        if (r.Kezdes == default || r.Kezdes.UtcDateTime < j.AtvetelDatuma || r.Kezdes > DateTimeOffset.UtcNow
            || r.Befejezes < r.Kezdes || r.Befejezes > DateTimeOffset.UtcNow)
            throw new SzervizKivetel(400, "A munka az átvétel után kezdődhet; a befejezés nem előzheti meg a kezdést és egyik időpont sem lehet jövőbeli. ISO 8601 időpontot adj meg időzónával.");
        return;
    }

    private static void Kitolt(Munkanaplo m, MunkanaploRequest r)
    {
        m.Leiras = r.Leiras.Trim(); m.Kezdes = r.Kezdes.UtcDateTime; m.Befejezes = r.Befejezes?.UtcDateTime;
        m.LedolgozottOra = r.Befejezes.HasValue ? decimal.Round((decimal)(r.Befejezes.Value - r.Kezdes).TotalHours, 4) : 0;
        m.Oradij = decimal.Round(r.Oradij, 2);
    }

    private static MunkanaploDto Adatok(Munkanaplo m) => new(m.Id, m.JavitasId, m.FelhasznaloId, m.Leiras,
        m.Kezdes, m.Befejezes, m.LedolgozottOra, m.Oradij, decimal.Round(m.LedolgozottOra * m.Oradij, 2));
}
