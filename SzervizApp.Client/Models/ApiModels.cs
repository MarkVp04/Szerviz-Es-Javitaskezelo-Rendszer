namespace SzervizApp.Client.Models;

public sealed record LoginRequest(string Felhasznalonev, string Jelszo);

public sealed class LoginResponseDto
{
    public string Token { get; set; } = "";
    public DateTime Lejarat { get; set; }
    public int FelhasznaloId { get; set; }
    public string Felhasznalonev { get; set; } = "";
    public string Nev { get; set; } = "";
    public string? Email { get; set; }
    public string Szerepkor { get; set; } = "";
    public int? UgyfelId { get; set; }
}

public sealed class JavitasDto
{
    public int Id { get; set; }
    public int UgyfelId { get; set; }
    public int EszkozId { get; set; }
    public string BejelentettHiba { get; set; } = "";
    public DateTime AtvetelDatuma { get; set; }
    public string Allapot { get; set; } = "";
    public DateTime? LezarasDatuma { get; set; }
    public string? Eredmeny { get; set; }
    public string? LezarasIndoklasa { get; set; }
    public int? FelvevoFelhasznaloId { get; set; }
    public int? FelelosFelhasznaloId { get; set; }
    public DateTime? TervezettBefejezes { get; set; }
    public bool Garancialis { get; set; }
    public string? Megjegyzes { get; set; }
    public bool KoltsegekVeglegesitve { get; set; }
    public int Verzio { get; set; }
}

public sealed class FeladatDto
{
    public int Id { get; set; }
    public int UgyfelId { get; set; }
    public string UgyfelNev { get; set; } = "";
    public int EszkozId { get; set; }
    public string EszkozTipus { get; set; } = "";
    public string BejelentettHiba { get; set; } = "";
    public string Allapot { get; set; } = "";
    public int? FelelosFelhasznaloId { get; set; }
    public string? FelelosNev { get; set; }
    public DateTime? TervezettBefejezes { get; set; }
    public DateTime AtvetelDatuma { get; set; }
}

public sealed class UgyfelDto
{
    public int Id { get; set; }
    public string Nev { get; set; } = "";
    public string Telefon { get; set; } = "";
    public string? Email { get; set; }
    public string? Cim { get; set; }
    public DateTime Letrehozva { get; set; }
    public bool Aktiv { get; set; }
}

public sealed class EszkozListaDto
{
    public int Id { get; set; }
    public int TulajdonosId { get; set; }
    public string EszkozTipus { get; set; } = "";
    public string? Gyarto { get; set; }
    public string? Sorozatszam { get; set; }
    public DateTime? VasarlasDatuma { get; set; }
    public string? Megjegyzes { get; set; }
    public DateTime Letrehozva { get; set; }
    public string Megjelenites => string.Join(" ", new[] { Gyarto, EszkozTipus, Sorozatszam is null ? null : $"({Sorozatszam})" }.Where(x => !string.IsNullOrWhiteSpace(x)));
}

public sealed class UgyfelRequest
{
    public string Nev { get; set; } = "";
    public string Telefon { get; set; } = "";
    public string? Email { get; set; }
    public string? Cim { get; set; }
    public bool Aktiv { get; set; } = true;
}

public sealed class EszkozRequest
{
    public int TulajdonosId { get; set; }
    public string EszkozTipus { get; set; } = "";
    public string? Gyarto { get; set; }
    public string? Sorozatszam { get; set; }
    public DateTimeOffset? VasarlasDatuma { get; set; }
    public string? Megjegyzes { get; set; }
}

public sealed class UjJavitasRequest
{
    public int UgyfelId { get; set; }
    public int EszkozId { get; set; }
    public string BejelentettHiba { get; set; } = "";
    public bool Garancialis { get; set; }
    public string? Megjegyzes { get; set; }
}

public sealed class ApiErrorResponse
{
    public string? Message { get; set; }
    public string? Title { get; set; }
}

public sealed record JavitasSor(int Id, string Ugyfel, string Eszkoz, string Hiba, string Allapot, DateTime Atvetel);
public sealed record UgyfelSor(int Id, string Nev, string Telefon, string Email, string Cim, string Allapot);
public sealed record EszkozSor(int Id, string Eszkoz, string Ugyfel, string Sorozatszam, string Vasarlas);

public sealed class FelhasznaloDto
{
    public int Id { get; set; }
    public string Felhasznalonev { get; set; } = "";
    public string Nev { get; set; } = "";
    public string? Email { get; set; }
    public string? Telefon { get; set; }
    public bool Aktiv { get; set; }
    public int JogosultsagId { get; set; }
    public string Szerepkor { get; set; } = "";
    public int? UgyfelId { get; set; }
}

public sealed class UjFelhasznaloRequest
{
    public string Felhasznalonev { get; set; } = "";
    public string Nev { get; set; } = "";
    public string? Email { get; set; }
    public string? Telefon { get; set; }
    public int JogosultsagId { get; set; }
    public int? UgyfelId { get; set; }
    public bool Aktiv { get; set; } = true;
    public string? UjJelszo { get; set; }
}

public sealed class BevizsgalasDto
{
    public int Id { get; set; }
    public int JavitasId { get; set; }
    public string Eredmeny { get; set; } = "";
    public decimal? BecsultKoltseg { get; set; }
    public DateTime Datum { get; set; }
    public int VegezteFelhasznaloId { get; set; }
}

public sealed class BevizsgalasRequest
{
    public string Eredmeny { get; set; } = "";
    public decimal? BecsultKoltseg { get; set; }
}

public sealed class MunkanaploDto
{
    public int Id { get; set; }
    public int JavitasId { get; set; }
    public int FelhasznaloId { get; set; }
    public string Leiras { get; set; } = "";
    public DateTime Kezdes { get; set; }
    public DateTime? Befejezes { get; set; }
    public decimal LedolgozottOra { get; set; }
    public decimal Oradij { get; set; }
    public decimal Munkadij { get; set; }
}

public sealed class MunkanaploRequest
{
    public string Leiras { get; set; } = "";
    public DateTimeOffset Kezdes { get; set; }
    public DateTimeOffset? Befejezes { get; set; }
    public decimal Oradij { get; set; }
}

public sealed class KoltsegDto
{
    public int Id { get; set; }
    public int JavitasId { get; set; }
    public string Megnevezes { get; set; } = "";
    public string Tipus { get; set; } = "Egyeb";
    public decimal Osszeg { get; set; }
    public DateTime Datum { get; set; }
    public int RogzitoFelhasznaloId { get; set; }
}

public sealed class KoltsegRequest
{
    public string Megnevezes { get; set; } = "";
    public string Tipus { get; set; } = "Egyeb";
    public decimal Osszeg { get; set; }
}

public sealed class KoltsegOsszesitesDto
{
    public int JavitasId { get; set; }
    public string Penznem { get; set; } = "HUF";
    public decimal Munkadij { get; set; }
    public decimal AlkatreszDij { get; set; }
    public decimal EgyebKoltseg { get; set; }
    public decimal Vegosszeg { get; set; }
    public bool Garancialis { get; set; }
    public bool KoltsegekVeglegesitve { get; set; }
    public int FolyamatbanLevoMunkak { get; set; }
    public List<KoltsegDto> Tetelek { get; set; } = [];
}

public sealed class AlkatreszDto
{
    public int Id { get; set; }
    public string Cikkszam { get; set; } = "";
    public string Nev { get; set; } = "";
    public string? Gyarto { get; set; }
    public string Egyseg { get; set; } = "db";
    public decimal AktualisEgysegar { get; set; }
    public bool Aktiv { get; set; }
    public string Megjelenites => $"{Cikkszam} – {Nev} ({AktualisEgysegar:N0} Ft/{Egyseg})";
}

public sealed class AlkatreszFelhasznalasDto
{
    public int Id { get; set; }
    public int JavitasId { get; set; }
    public int AlkatreszId { get; set; }
    public string Cikkszam { get; set; } = "";
    public string Nev { get; set; } = "";
    public string Egyseg { get; set; } = "db";
    public decimal Mennyiseg { get; set; }
    public decimal Egysegar { get; set; }
    public decimal Osszeg { get; set; }
    public DateTime Datum { get; set; }
    public int RogzitoFelhasznaloId { get; set; }
}

public sealed class AlkatreszFelhasznalasRequest
{
    public int AlkatreszId { get; set; }
    public decimal Mennyiseg { get; set; }
}

public sealed class FelelosRequest
{
    public int? FelhasznaloId { get; set; }
    public DateTimeOffset? TervezettBefejezes { get; set; }
}

public sealed class AllapotRequest
{
    public string Allapot { get; set; } = "";
}

public sealed class JavitasLezarasRequest
{
    public string Eredmeny { get; set; } = "Javitva";
    public string LezarasIndoklasa { get; set; } = "";
}
