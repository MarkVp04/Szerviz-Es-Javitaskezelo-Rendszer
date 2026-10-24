using System.ComponentModel.DataAnnotations;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Contracts;

public sealed class JavitasLezarasRequest
{
    [EnumDataType(typeof(JavitasEredmeny))]
    public JavitasEredmeny Eredmeny { get; set; }

    [Required, StringLength(2000)]
    public string? LezarasIndoklasa { get; set; }
}
