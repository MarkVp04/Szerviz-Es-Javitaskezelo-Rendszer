using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DTOs;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;
using Microsoft.AspNetCore.Identity;

namespace SzervizApp.Services;

public interface IFelhasznaloService
{
    Task<IEnumerable<FelhasznaloDto>> Lista(CancellationToken ct);
    Task<FelhasznaloDto> Egy(int id, CancellationToken ct);
    Task<FelhasznaloDto> Uj(UjFelhasznaloRequest r, CancellationToken ct);
    Task<FelhasznaloDto> Modosit(int id, FelhasznaloRequest r, CancellationToken ct);
}

public sealed class FelhasznaloService(AppDbContext db, IHttpContextAccessor accessor, IPasswordHasher<Felhasznalo> hasher)
    : SzervizServiceBase(db, accessor), IFelhasznaloService
{
    public async Task<IEnumerable<FelhasznaloDto>> Lista(CancellationToken ct) => (await Db.Felhasznalok
        .AsNoTracking().Include(x => x.Jogosultsag).OrderBy(x => x.Nev).ToListAsync(ct)).Select(FelhasznaloAdatok.ToDto);

    public async Task<FelhasznaloDto> Egy(int id, CancellationToken ct)
    {
        var f = await Db.Felhasznalok.AsNoTracking().Include(x => x.Jogosultsag).SingleOrDefaultAsync(x => x.Id == id, ct);
        return f is null ? throw new SzervizKivetel(404, "Az adat nem található.") : FelhasznaloAdatok.ToDto(f);
    }

    public async Task<FelhasznaloDto> Uj(UjFelhasznaloRequest r, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(r.UjJelszo)) throw new SzervizKivetel(400, "Az új felhasználóhoz jelszó kell (legalább 10 karakter).");
        await EllenorizAdatokat(r, ct);
        var normalizalt = r.Felhasznalonev.Trim().ToUpperInvariant();
        if (await Db.Felhasznalok.AnyAsync(x => x.NormalizaltFelhasznalonev == normalizalt, ct))
            throw new SzervizKivetel(409, "Ez a felhasználónév már foglalt.");
        var f = new Felhasznalo { Felhasznalonev = r.Felhasznalonev.Trim(), NormalizaltFelhasznalonev = normalizalt };
        Kitolt(f, r);
        Db.Felhasznalok.Add(f);
        await Db.SaveChangesAsync(ct);
        await Db.Entry(f).Reference(x => x.Jogosultsag).LoadAsync(ct);
        return FelhasznaloAdatok.ToDto(f);
    }

    public async Task<FelhasznaloDto> Modosit(int id, FelhasznaloRequest r, CancellationToken ct)
    {
        var f = await Db.Felhasznalok.Include(x => x.Jogosultsag).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (f is null) throw new SzervizKivetel(404, "Az adat nem található.");
        await EllenorizAdatokat(r, ct);
        if (f.Aktiv && f.JogosultsagId == 3 && (!r.Aktiv || r.JogosultsagId != 3)
            && !await Db.Felhasznalok.AnyAsync(x => x.Id != id && x.Aktiv && x.JogosultsagId == 3, ct))
            throw new SzervizKivetel(409, "Az utolsó aktív manager nem kapcsolható ki.");
        if ((!r.Aktiv || r.JogosultsagId is not (2 or 3)) && await Db.Javitasok.AnyAsync(
            x => x.FelelosFelhasznaloId == id && x.Allapot != JavitasAllapot.Lezart && x.Allapot != JavitasAllapot.Visszaadva, ct))
            throw new SzervizKivetel(409, "Előbb rendeld másik felelőshöz a munkatárs nyitott javításait.");
        Kitolt(f, r);
        f.BelepesVerzio = Guid.NewGuid();
        await Db.SaveChangesAsync(ct);
        await Db.Entry(f).Reference(x => x.Jogosultsag).LoadAsync(ct);
        return FelhasznaloAdatok.ToDto(f);
    }

    private async Task EllenorizAdatokat(FelhasznaloRequest r, CancellationToken ct)
    {
        if (r.JogosultsagId == 4)
        {
            if (!await Db.Ugyfelek.AnyAsync(x => x.Id == r.UgyfelId && x.Aktiv, ct))
                throw new SzervizKivetel(400, "Ügyfélfiókhoz aktív ügyfelet kell megadni.");
        }
        else if (r.UgyfelId is not null)
            throw new SzervizKivetel(400, "Csak ügyfél szerepkörhöz tartozhat ügyfélazonosító.");
        if (r.UjJelszo is not null && string.IsNullOrWhiteSpace(r.UjJelszo)) throw new SzervizKivetel(400, "Hibás kérés.");
        return;
    }

    private void Kitolt(Felhasznalo f, FelhasznaloRequest r)
    {
        f.Nev = r.Nev.Trim(); f.Email = Szoveg(r.Email); f.Telefon = Szoveg(r.Telefon);
        f.JogosultsagId = r.JogosultsagId; f.UgyfelId = r.UgyfelId; f.Aktiv = r.Aktiv;
        if (r.UjJelszo is not null) f.JelszoHash = hasher.HashPassword(f, r.UjJelszo);
    }
}
