using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzervizApp.DTOs;
using SzervizApp.Services;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Controllers;

[Route("api/ugyfelek")]
public sealed class UgyfelekController : SzervizControllerBase
{
    private readonly IUgyfelService _service;

    public UgyfelekController(IUgyfelService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ugyfel>>> Lista(CancellationToken ct, string? kereses = null, bool? aktiv = null)
    => Ok(await _service.Lista(ct, kereses, aktiv));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Ugyfel>> Egy(int id, CancellationToken ct)
    => Ok(await _service.Egy(id, ct));

    [HttpPost, Authorize(Roles = Recepcio)]
    public async Task<ActionResult<Ugyfel>> Uj(UgyfelRequest r, CancellationToken ct)
    {
        var result = await _service.Uj(r, ct);
        return CreatedAtAction(nameof(Egy), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}"), Authorize(Roles = Recepcio)]
    public async Task<ActionResult<Ugyfel>> Modosit(int id, UgyfelRequest r, CancellationToken ct)
    => Ok(await _service.Modosit(id, r, ct));
}
