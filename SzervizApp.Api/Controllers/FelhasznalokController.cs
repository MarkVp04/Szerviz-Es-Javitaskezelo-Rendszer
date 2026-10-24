using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzervizApp.DTOs;
using SzervizApp.Services;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Controllers;

[Route("api/felhasznalok"), Authorize(Roles = Szerepkorok.Manager)]
public sealed class FelhasznalokController : SzervizControllerBase
{
    private readonly IFelhasznaloService _service;

    public FelhasznalokController(IFelhasznaloService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FelhasznaloDto>>> Lista(CancellationToken ct)
    => Ok(await _service.Lista(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FelhasznaloDto>> Egy(int id, CancellationToken ct)
    => Ok(await _service.Egy(id, ct));

    [HttpPost]
    public async Task<ActionResult<FelhasznaloDto>> Uj(UjFelhasznaloRequest r, CancellationToken ct)
    {
        var result = await _service.Uj(r, ct);
        return CreatedAtAction(nameof(Egy), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FelhasznaloDto>> Modosit(int id, FelhasznaloRequest r, CancellationToken ct)
    => Ok(await _service.Modosit(id, r, ct));
}
