using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SzervizApp.DTOs;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Services;

public abstract class SzervizServiceBase(AppDbContext db, IHttpContextAccessor accessor)
{
    protected readonly AppDbContext Db = db;
    protected ClaimsPrincipal User => accessor.HttpContext?.User ?? throw new SzervizKivetel(401, "Belépés szükséges.");
    protected int FelhasznaloId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new SzervizKivetel(401, "Belépés szükséges.");
    protected bool Ugyfel => User.IsInRole(Szerepkorok.Ugyfel);
    protected bool Manager => User.IsInRole(Szerepkorok.Manager);
    protected int? UgyfelId => int.TryParse(User.FindFirstValue("ugyfelId"), out var id) ? id : null;
    protected static string? Szoveg(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    protected IQueryable<Javitas> LathatoJavitasok => Ugyfel
        ? Db.Javitasok.Where(j => j.UgyfelId == UgyfelId) : Db.Javitasok;

    protected void Ellenoriz([NotNull] Javitas? javitas, bool modositas = false)
    {
        if (javitas is null || (Ugyfel && javitas.UgyfelId != UgyfelId))
            throw new SzervizKivetel(404, "A javítás nem található.");
        if (!modositas) return;
        if (!Manager && (!User.IsInRole(Szerepkorok.Szerelo) || javitas.FelelosFelhasznaloId != FelhasznaloId))
            throw new SzervizKivetel(403, "Csak a kijelölt szerelő vagy a manager módosíthatja.");
        Nyitott(javitas);
    }

    protected static void Nyitott(Javitas javitas)
    {
        if (javitas.Allapot is JavitasAllapot.Lezart or JavitasAllapot.Visszaadva)
            throw new SzervizKivetel(409, "A lezárt javítás már nem módosítható.");
    }

    protected static void Valtozott(Javitas javitas, bool koltsegValtozott = false)
    {
        javitas.Verzio++;
        if (koltsegValtozott) javitas.KoltsegekVeglegesitve = false;
    }

    protected static JavitasDto ToDto(Javitas j) => new(j.Id, j.UgyfelId, j.EszkozId,
        j.BejelentettHiba, j.AtvetelDatuma, j.Allapot, j.LezarasDatuma, j.Eredmeny,
        j.LezarasIndoklasa, j.FelvevoFelhasznaloId, j.FelelosFelhasznaloId,
        j.TervezettBefejezes, j.Garancialis, j.Megjegyzes, j.KoltsegekVeglegesitve, j.Verzio);
}
