using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DTOs;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Services;

public interface IJavitasService
{
    Task<IEnumerable<JavitasDto>> GetAll(CancellationToken ct, string? kereses = null,
        JavitasAllapot? allapot = null, int? felelosId = null, bool csakNyitott = false);
    Task<IEnumerable<FeladatDto>> Feladatok(CancellationToken ct, bool sajat = false);
    Task<JavitasDto> GetById(int id, CancellationToken ct);
    Task<JavitasDto> Create(UjJavitasRequest r, CancellationToken ct);
    Task<JavitasDto> Felelos(int id, FelelosRequest r, CancellationToken ct);
    Task<JavitasDto> Vallalas(int id, CancellationToken ct);
    Task<JavitasDto> Allapot(int id, AllapotRequest r, CancellationToken ct);
    Task<JavitasDto> Lezaras(int id, JavitasLezarasRequest r, CancellationToken ct);
    Task<Visszaadas> VisszaadasAdatok(int id, CancellationToken ct);
    Task<VisszaadasDto> Visszaadas(int id, VisszaadasRequest r, CancellationToken ct);
}

public sealed class JavitasService(AppDbContext db, IHttpContextAccessor accessor)
    : SzervizServiceBase(db, accessor), IJavitasService
{
    public async Task<IEnumerable<JavitasDto>> GetAll(CancellationToken ct, string? kereses = null,
        JavitasAllapot? allapot = null, int? felelosId = null, bool csakNyitott = false)
    {
        if (allapot.HasValue && !Enum.IsDefined(allapot.Value)) throw new SzervizKivetel(400, "Hibás kérés.");
        var q = LathatoJavitasok.AsNoTracking();
        if (allapot.HasValue) q = q.Where(j => j.Allapot == allapot);
        if (felelosId.HasValue) q = q.Where(j => j.FelelosFelhasznaloId == felelosId);
        if (csakNyitott) q = q.Where(j => j.Allapot != JavitasAllapot.Lezart && j.Allapot != JavitasAllapot.Visszaadva);
        if (!string.IsNullOrWhiteSpace(kereses)) {
            var k = kereses.Trim();
            q = q.Where(j => j.BejelentettHiba.Contains(k) || j.Ugyfel.Nev.Contains(k)
                || j.Eszkoz.EszkozTipus.Contains(k) || (j.Eszkoz.Sorozatszam != null && j.Eszkoz.Sorozatszam.Contains(k)));
        }
        return (await q.OrderByDescending(j => j.AtvetelDatuma).ToListAsync(ct)).Select(ToDto);
    }

    public async Task<IEnumerable<FeladatDto>> Feladatok(CancellationToken ct, bool sajat = false)
    {
        var q = Db.Javitasok.AsNoTracking().Where(j => j.Allapot != JavitasAllapot.Visszaadva);
        if (sajat) q = q.Where(j => j.FelelosFelhasznaloId == FelhasznaloId);
        return await q.OrderBy(j => j.TervezettBefejezes).ThenBy(j => j.AtvetelDatuma).Select(j => new FeladatDto(
            j.Id, j.UgyfelId, j.Ugyfel.Nev, j.EszkozId, j.Eszkoz.EszkozTipus,
            j.BejelentettHiba, j.Allapot, j.FelelosFelhasznaloId,
            j.FelelosFelhasznalo == null ? null : j.FelelosFelhasznalo.Nev,
            j.TervezettBefejezes, j.AtvetelDatuma
        )).ToListAsync(ct);
    }

    public async Task<JavitasDto> GetById(int id, CancellationToken ct)
    {
        var j = await LathatoJavitasok.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return j is null ? throw new SzervizKivetel(404, "Az adat nem található.") : ToDto(j);
    }

    public async Task<JavitasDto> Create(UjJavitasRequest r, CancellationToken ct)
    {
        if (!await Db.Ugyfelek.AnyAsync(u => u.Id == r.UgyfelId && u.Aktiv, ct))
            throw new SzervizKivetel(404, "A megadott aktív ügyfél nem található.");
        var e = await Db.Eszkozok.FindAsync([r.EszkozId], ct);
        if (e is null) throw new SzervizKivetel(404, "A megadott eszköz nem található.");
        if (e.TulajdonosId != r.UgyfelId)
            throw new SzervizKivetel(400, "Az eszköz nem a megadott ügyfélhez tartozik.");
        var j = new Javitas {
            UgyfelId = r.UgyfelId, EszkozId = r.EszkozId, BejelentettHiba = r.BejelentettHiba.Trim(),
            FelvevoFelhasznaloId = FelhasznaloId, Garancialis = r.Garancialis, Megjegyzes = Szoveg(r.Megjegyzes)
        };
        Db.Javitasok.Add(j); await Db.SaveChangesAsync(ct);
        return ToDto(j);
    }

    public async Task<JavitasDto> Felelos(int id, FelelosRequest r, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([id], ct); if (j is null) throw new SzervizKivetel(404, "Az adat nem található.");
        Nyitott(j);
        if (r.FelhasznaloId.HasValue && !await Db.Felhasznalok.AnyAsync(f => f.Id == r.FelhasznaloId
            && f.Aktiv && (f.Jogosultsag.Kod == Szerepkorok.Szerelo || f.Jogosultsag.Kod == Szerepkorok.Manager), ct))
            throw new SzervizKivetel(400, "Felelősnek csak aktív szerelő vagy manager választható.");
        if (r.TervezettBefejezes?.UtcDateTime < j.AtvetelDatuma)
            throw new SzervizKivetel(400, "A tervezett befejezés nem előzheti meg az átvételt.");
        j.FelelosFelhasznaloId = r.FelhasznaloId; j.TervezettBefejezes = r.TervezettBefejezes?.UtcDateTime;
        Valtozott(j); await Db.SaveChangesAsync(ct); return ToDto(j);
    }

    public async Task<JavitasDto> Vallalas(int id, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([id], ct);
        if (j is null) throw new SzervizKivetel(404, "A javítás nem található.");
        Nyitott(j);
        if (j.FelelosFelhasznaloId.HasValue)
            throw new SzervizKivetel(409, "Ezt a javítást már vállalta vagy megkapta egy másik munkatárs.");

        // Csak a bejelentkezett szerelő vállalhatja a munkát. A controller szerepkör-ellenőrzése
        // mellett itt is explicit szerepkör-ellenőrzést végzünk, hogy a szolgáltatás szabálya egyértelmű legyen.
        if (!User.IsInRole(Szerepkorok.Szerelo))
            throw new SzervizKivetel(403, "Feladatot csak szerelő vállalhat.");

        j.FelelosFelhasznaloId = FelhasznaloId;
        Valtozott(j); // A Verzio concurrency token megakadályozza a párhuzamos dupla vállalást.
        await Db.SaveChangesAsync(ct);
        return ToDto(j);
    }

    public async Task<JavitasDto> Allapot(int id, AllapotRequest r, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([id], ct);
        Ellenoriz(j, true);
        if (!j!.FelelosFelhasznaloId.HasValue) throw new SzervizKivetel(409, "Előbb jelölj ki felelőst.");
        var megengedett = j.Allapot switch {
            JavitasAllapot.Atvett => r.Allapot == JavitasAllapot.BevizsgalasAlatt,
            JavitasAllapot.BevizsgalasAlatt => r.Allapot is JavitasAllapot.JavitasAlatt or JavitasAllapot.AlkatreszreVar,
            JavitasAllapot.JavitasAlatt => r.Allapot == JavitasAllapot.AlkatreszreVar,
            JavitasAllapot.AlkatreszreVar => r.Allapot == JavitasAllapot.JavitasAlatt,
            _ => false
        };
        if (!megengedett) throw new SzervizKivetel(409, "Ez az állapotváltás nem megengedett. Lezáráshoz és visszaadáshoz a külön végpontot használd.");
        if (r.Allapot is JavitasAllapot.JavitasAlatt or JavitasAllapot.AlkatreszreVar
            && !await Db.Bevizsgalasok.AnyAsync(b => b.JavitasId == id, ct))
            throw new SzervizKivetel(409, "Előbb rögzítsd a bevizsgálás eredményét.");
        j.Allapot = r.Allapot; Valtozott(j); await Db.SaveChangesAsync(ct); return ToDto(j);
    }

    public async Task<JavitasDto> Lezaras(int id, JavitasLezarasRequest r, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([id], ct);
        Ellenoriz(j, true);
        if (!j!.FelelosFelhasznaloId.HasValue) throw new SzervizKivetel(409, "A lezáráshoz felelős kell.");
        if (!j.KoltsegekVeglegesitve) throw new SzervizKivetel(409, "Előbb véglegesítsd a költségeket (nulla összegnél is).");
        if (await Db.Munkanaplok.AnyAsync(m => m.JavitasId == id && m.Befejezes == null, ct))
            throw new SzervizKivetel(409, "Előbb fejezd be a megkezdett munkanapló-bejegyzéseket.");
        if (r.Eredmeny != JavitasEredmeny.JavitasNelkulLezarva
            && !await Db.Bevizsgalasok.AnyAsync(b => b.JavitasId == id, ct))
            throw new SzervizKivetel(409, "Ehhez az eredményhez bevizsgálás kell.");
        if (r.Eredmeny == JavitasEredmeny.Javitva && !await Db.Munkanaplok.AnyAsync(m => m.JavitasId == id && m.Befejezes != null, ct))
            throw new SzervizKivetel(409, "Sikeres javításhoz legalább egy befejezett munkát rögzíts.");
        j.Allapot = JavitasAllapot.Lezart; j.LezarasDatuma = DateTime.UtcNow;
        j.Eredmeny = r.Eredmeny; j.LezarasIndoklasa = r.LezarasIndoklasa!.Trim();
        Valtozott(j); await Db.SaveChangesAsync(ct); return ToDto(j);
    }

    public async Task<Visszaadas> VisszaadasAdatok(int id, CancellationToken ct)
    {
        var j = await LathatoJavitasok.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (j is null) throw new SzervizKivetel(404, "Az adat nem található.");
        var v = await Db.Visszaadasok.AsNoTracking().SingleOrDefaultAsync(x => x.JavitasId == id, ct);
        return v is null ? throw new SzervizKivetel(404, "Az adat nem található.") : v;
    }

    public async Task<VisszaadasDto> Visszaadas(int id, VisszaadasRequest r, CancellationToken ct)
    {
        var j = await Db.Javitasok.FindAsync([id], ct); if (j is null) throw new SzervizKivetel(404, "Az adat nem található.");
        if (j.Allapot != JavitasAllapot.Lezart) throw new SzervizKivetel(409, "Csak lezárt, még vissza nem adott javítás adható át.");
        var v = new Visszaadas { JavitasId = id, RogzitoFelhasznaloId = FelhasznaloId,
            AtvevoNev = r.AtvevoNev.Trim(), Megjegyzes = Szoveg(r.Megjegyzes) };
        Db.Visszaadasok.Add(v); j.Allapot = JavitasAllapot.Visszaadva; Valtozott(j);
        await Db.SaveChangesAsync(ct);
        return new VisszaadasDto(v.Id, v.JavitasId,
            v.RogzitoFelhasznaloId, v.VisszaadasDatuma, v.AtvevoNev, v.Megjegyzes);
    }
}
