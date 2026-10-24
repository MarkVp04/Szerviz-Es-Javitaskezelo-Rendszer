using System.ComponentModel.DataAnnotations;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Contracts;

public sealed class FelelosRequest
{
    public int? FelhasznaloId { get; set; }
    public DateTimeOffset? TervezettBefejezes { get; set; }
}

public sealed class AllapotRequest
{
    [EnumDataType(typeof(JavitasAllapot))] public JavitasAllapot Allapot { get; set; }
}

public sealed class BevizsgalasRequest
{
    [Required, StringLength(4000)] public string Eredmeny { get; set; } = "";
    [Range(typeof(decimal), "0", "100000000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] public decimal? BecsultKoltseg { get; set; }
}

public sealed class MunkanaploRequest
{
    [Required, StringLength(4000)] public string Leiras { get; set; } = "";
    public DateTimeOffset Kezdes { get; set; }
    public DateTimeOffset? Befejezes { get; set; }
    [Range(typeof(decimal), "0", "10000000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] public decimal Oradij { get; set; }
}

public sealed class AlkatreszFelhasznalasRequest
{
    [Range(1, int.MaxValue)] public int AlkatreszId { get; set; }
    [Range(typeof(decimal), "0.001", "100000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] public decimal Mennyiseg { get; set; }
}

public sealed class KoltsegRequest
{
    [Required, StringLength(200)] public string Megnevezes { get; set; } = "";
    [EnumDataType(typeof(KoltsegTipus))] public KoltsegTipus Tipus { get; set; }
    [Range(typeof(decimal), "0", "100000000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] public decimal Osszeg { get; set; }
}

public sealed class VisszaadasRequest
{
    [Required, StringLength(150)] public string AtvevoNev { get; set; } = "";
    [StringLength(2000)] public string? Megjegyzes { get; set; }
}

public sealed class UzenetRequest
{
    public int? UgyfelId { get; set; }
    public int? JavitasId { get; set; }
    [Required, StringLength(4000)] public string Tartalom { get; set; } = "";
}
