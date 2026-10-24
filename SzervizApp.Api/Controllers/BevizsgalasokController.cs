using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzervizApp.DTOs;
using SzervizApp.Services;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Controllers;

[Route("api/javitasok/{javitasId:int}/bevizsgalas")]
public sealed class BevizsgalasokController : SzervizControllerBase
{
    private readonly IBevizsgalasService _service;

    public BevizsgalasokController(IBevizsgalasService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<BevizsgalasDto>> Egy(int javitasId, CancellationToken ct)
    => Ok(await _service.Egy(javitasId, ct));

    [HttpPost, Authorize(Roles = Technikus)]
    public async Task<ActionResult<BevizsgalasDto>> Uj(int javitasId, BevizsgalasRequest r, CancellationToken ct)
    {
        var result = await _service.Uj(javitasId, r, ct);
        return CreatedAtAction(nameof(Egy), new { javitasId }, result);
    }

    [HttpPut, Authorize(Roles = Technikus)]
    public async Task<ActionResult<BevizsgalasDto>> Modosit(int javitasId, BevizsgalasRequest r, CancellationToken ct)
    => Ok(await _service.Modosit(javitasId, r, ct));
}
