using System.ComponentModel.DataAnnotations;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Contracts;

public sealed class JavitasLezarasRequest
{
    public JavitasEredmeny Eredmeny { get; set; }

    [StringLength(2000)]
    public string? LezarasIndoklasa { get; set; }
}