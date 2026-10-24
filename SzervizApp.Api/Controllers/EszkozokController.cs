using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzervizApp.DTOs;
using SzervizApp.Services;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Controllers;

[Route("api/eszkozok")]
public sealed class EszkozokController : SzervizControllerBase
{
    private readonly IEszkozService _service;

    public EszkozokController(IEszkozService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EszkozListaDto>>> Lista(CancellationToken ct, string? kereses = null, int? tulajdonosId = null)
    => Ok(await _service.Lista(ct, kereses, tulajdonosId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Eszkoz>> Egy(int id, CancellationToken ct)
    => Ok(await _service.Egy(id, ct));

    [HttpGet("{id:int}/javitasok")]
    public async Task<ActionResult<IEnumerable<JavitasDto>>> Elozmenyek(int id, CancellationToken ct)
    => Ok(await _service.Elozmenyek(id, ct));

    [HttpPost, Authorize(Roles = Recepcio)]
    public async Task<ActionResult<Eszkoz>> Uj(EszkozRequest r, CancellationToken ct)
    {
        var result = await _service.Uj(r, ct);
        return CreatedAtAction(nameof(Egy), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}"), Authorize(Roles = Recepcio)]
    public async Task<ActionResult<Eszkoz>> Modosit(int id, EszkozRequest r, CancellationToken ct)
    => Ok(await _service.Modosit(id, r, ct));
}
