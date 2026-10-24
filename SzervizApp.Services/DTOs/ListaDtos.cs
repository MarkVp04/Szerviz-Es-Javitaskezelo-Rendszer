using SzervizApp.DataContext.Entities;

namespace SzervizApp.DTOs;

public sealed record EszkozListaDto(int Id, int TulajdonosId, string EszkozTipus,
    string? Gyarto, string? Sorozatszam, DateTime? VasarlasDatuma, string? Megjegyzes, DateTime Letrehozva);
public sealed record FeladatDto(int Id, int UgyfelId, string UgyfelNev, int EszkozId,
    string EszkozTipus, string BejelentettHiba, JavitasAllapot Allapot, int? FelelosFelhasznaloId,
    string? FelelosNev, DateTime? TervezettBefejezes, DateTime AtvetelDatuma);
public sealed record KoltsegOsszesitesDto(int JavitasId, string Penznem, decimal Munkadij,
    decimal AlkatreszDij, decimal EgyebKoltseg, decimal Vegosszeg, bool Garancialis,
    bool KoltsegekVeglegesitve, int FolyamatbanLevoMunkak, IEnumerable<KoltsegDto> Tetelek);
public sealed record UzenetListaDto(int Id, int UgyfelId, int? JavitasId, int FeladoFelhasznaloId,
    string FeladoNev, string Tartalom, DateTime KuldesDatuma);
