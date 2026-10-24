using SzervizApp.DataContext.Entities;

namespace SzervizApp.DTOs;

public sealed record FelhasznaloDto(int Id, string Felhasznalonev, string Nev, string? Email,
    string? Telefon, bool Aktiv, int JogosultsagId, string Szerepkor, int? UgyfelId);
public sealed record LoginResponseDto(string Token, DateTime Lejarat, int FelhasznaloId,
    string Felhasznalonev, string Nev, string? Email, string Szerepkor, int? UgyfelId);
public sealed record BevizsgalasDto(int Id, int JavitasId, string Eredmeny, decimal? BecsultKoltseg,
    DateTime Datum, int VegezteFelhasznaloId);
public sealed record MunkanaploDto(int Id, int JavitasId, int FelhasznaloId, string Leiras,
    DateTime Kezdes, DateTime? Befejezes, decimal LedolgozottOra, decimal Oradij, decimal Munkadij);
public sealed record AlkatreszFelhasznalasDto(int Id, int JavitasId, int AlkatreszId,
    string Cikkszam, string Nev, string Egyseg, decimal Mennyiseg, decimal Egysegar,
    decimal Osszeg, DateTime Datum, int RogzitoFelhasznaloId);
public sealed record KoltsegDto(int Id, int JavitasId, string Megnevezes, KoltsegTipus Tipus,
    decimal Osszeg, DateTime Datum, int RogzitoFelhasznaloId);
public sealed record VisszaadasDto(int Id, int JavitasId, int RogzitoFelhasznaloId,
    DateTime VisszaadasDatuma, string AtvevoNev, string? Megjegyzes);
public sealed record UzenetDto(int Id, int UgyfelId, int? JavitasId, int FeladoFelhasznaloId,
    string Tartalom, DateTime KuldesDatuma);
