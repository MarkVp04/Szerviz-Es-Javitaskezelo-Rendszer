using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Contracts;

public sealed record JavitasDto(
    int Id,
    int UgyfelId,
    int EszkozId,
    string BejelentettHiba,
    DateTime AtvetelDatuma,
    JavitasAllapot Allapot,
    DateTime? LezarasDatuma,
    JavitasEredmeny? Eredmeny,
    string? LezarasIndoklasa
);