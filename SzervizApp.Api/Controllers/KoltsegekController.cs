using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzervizApp.DTOs;
using SzervizApp.Services;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Controllers;

[Route("api/javitasok/{javitasId:int}/koltsegek")]
public sealed class KoltsegekController : SzervizControllerBase
{
    private readonly IKoltsegService _service;

    public KoltsegekController(IKoltsegService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<KoltsegOsszesitesDto>> Osszesites(int javitasId, CancellationToken ct)
    => Ok(await _service.Osszesites(javitasId, ct));

    [HttpPost, Authorize(Roles = Technikus)]
    public async Task<ActionResult<KoltsegDto>> Uj(int javitasId, KoltsegRequest r, CancellationToken ct)
    => Created($"/api/javitasok/{javitasId}/koltsegek", await _service.Uj(javitasId, r, ct));

    [HttpPut("{id:int}"), Authorize(Roles = Technikus)]
    public async Task<ActionResult<KoltsegDto>> Modosit(int javitasId, int id, KoltsegRequest r, CancellationToken ct)
    => Ok(await _service.Modosit(javitasId, id, r, ct));

    [HttpDelete("{id:int}"), Authorize(Roles = Technikus)]
    public async Task<ActionResult> Torol(int javitasId, int id, CancellationToken ct)
    {
        await _service.Torol(javitasId, id, ct);
        return NoContent();
    }

    [HttpPut("veglegesites"), Authorize(Roles = Technikus)]
    public async Task<ActionResult<KoltsegOsszesitesDto>> Veglegesites(int javitasId, CancellationToken ct)
    => Ok(await _service.Veglegesites(javitasId, ct));
}
