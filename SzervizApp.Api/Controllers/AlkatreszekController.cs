using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzervizApp.DTOs;
using SzervizApp.Services;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Controllers;

[Route("api/alkatreszek"), Authorize(Roles = Munkatarsak)]
public sealed class AlkatreszekController : SzervizControllerBase
{
    private readonly IAlkatreszService _service;

    public AlkatreszekController(IAlkatreszService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Alkatresz>>> Lista(CancellationToken ct, string? kereses = null, bool? aktiv = null)
    => Ok(await _service.Lista(ct, kereses, aktiv));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Alkatresz>> Egy(int id, CancellationToken ct)
    => Ok(await _service.Egy(id, ct));

    [HttpPost, Authorize(Roles = Szerepkorok.Manager)]
    public async Task<ActionResult<Alkatresz>> Uj(AlkatreszRequest r, CancellationToken ct)
    {
        var result = await _service.Uj(r, ct);
        return CreatedAtAction(nameof(Egy), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}"), Authorize(Roles = Szerepkorok.Manager)]
    public async Task<ActionResult<Alkatresz>> Modosit(int id, AlkatreszRequest r, CancellationToken ct)
    => Ok(await _service.Modosit(id, r, ct));
}
