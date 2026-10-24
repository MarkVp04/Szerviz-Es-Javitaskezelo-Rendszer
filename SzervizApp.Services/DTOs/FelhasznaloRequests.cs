using System.ComponentModel.DataAnnotations;

namespace SzervizApp.DTOs;

public sealed class BelepesRequest
{
    [Required, StringLength(80)] public string Felhasznalonev { get; set; } = "";
    [Required, StringLength(128)] public string Jelszo { get; set; } = "";
}

public class FelhasznaloRequest
{
    [Required, StringLength(150)] public string Nev { get; set; } = "";
    [EmailAddress, StringLength(254)] public string? Email { get; set; }
    [StringLength(30)] public string? Telefon { get; set; }
    [Range(1, 4)] public int JogosultsagId { get; set; }
    public int? UgyfelId { get; set; }
    public bool Aktiv { get; set; } = true;
    [StringLength(128, MinimumLength = 10)] public string? UjJelszo { get; set; }
}

public sealed class UjFelhasznaloRequest : FelhasznaloRequest
{
    [Required, StringLength(80)] public string Felhasznalonev { get; set; } = "";
}

public sealed class JelszoRequest
{
    [Required, StringLength(128)] public string RegiJelszo { get; set; } = "";
    [Required, StringLength(128, MinimumLength = 10)] public string UjJelszo { get; set; } = "";
}

public sealed class JogosultsagRequest
{
    [Required, StringLength(80)] public string Nev { get; set; } = "";
    [StringLength(500)] public string? Leiras { get; set; }
}
