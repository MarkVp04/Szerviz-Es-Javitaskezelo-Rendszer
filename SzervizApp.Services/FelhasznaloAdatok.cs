using SzervizApp.DTOs;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Services;

public static class FelhasznaloAdatok
{
    public static FelhasznaloDto ToDto(Felhasznalo f) => new(f.Id, f.Felhasznalonev,
        f.Nev, f.Email, f.Telefon, f.Aktiv, f.JogosultsagId, f.Jogosultsag.Kod, f.UgyfelId);
}
