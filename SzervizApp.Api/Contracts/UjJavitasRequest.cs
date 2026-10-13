using System.ComponentModel.DataAnnotations;

namespace SzervizApp.Api.Contracts;

public sealed class UjJavitasRequest
{
    [Range(1, int.MaxValue)]
    public int UgyfelId { get; set; }

    [Range(1, int.MaxValue)]
    public int EszkozId { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string BejelentettHiba { get; set; } = string.Empty;
}