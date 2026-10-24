using System.ComponentModel.DataAnnotations;

namespace SzervizApp.Api.Contracts;

public sealed class UgyfelRequest
{
    [Required, StringLength(150)] public string Nev { get; set; } = "";
    [Required, StringLength(30)] public string Telefon { get; set; } = "";
    [EmailAddress, StringLength(254)] public string? Email { get; set; }
    [StringLength(300)] public string? Cim { get; set; }
    public bool Aktiv { get; set; } = true;
}

public sealed class EszkozRequest
{
    [Range(1, int.MaxValue)] public int TulajdonosId { get; set; }
    [Required, StringLength(100)] public string EszkozTipus { get; set; } = "";
    [StringLength(100)] public string? Gyarto { get; set; }
    [StringLength(100)] public string? Sorozatszam { get; set; }
    public DateTimeOffset? VasarlasDatuma { get; set; }
    [StringLength(2000)] public string? Megjegyzes { get; set; }
}

public sealed class AlkatreszRequest
{
    [Required, StringLength(80)] public string Cikkszam { get; set; } = "";
    [Required, StringLength(150)] public string Nev { get; set; } = "";
    [StringLength(100)] public string? Gyarto { get; set; }
    [Required, StringLength(20)] public string Egyseg { get; set; } = "db";
    [Range(typeof(decimal), "0", "100000000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] public decimal AktualisEgysegar { get; set; }
    public bool Aktiv { get; set; } = true;
}
