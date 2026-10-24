using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzervizApp.DTOs;
using SzervizApp.Services;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Controllers;

[Route("api/javitasok/{javitasId:int}/alkatreszek")]
public sealed class AlkatreszFelhasznalasokController : SzervizControllerBase
{
    private readonly IAlkatreszFelhasznalasService _service;

    public AlkatreszFelhasznalasokController(IAlkatreszFelhasznalasService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AlkatreszFelhasznalasDto>>> Lista(int javitasId, CancellationToken ct)
    => Ok(await _service.Lista(javitasId, ct));

    [HttpPost, Authorize(Roles = Technikus)]
    public async Task<ActionResult<AlkatreszFelhasznalasDto>> Uj(int javitasId, AlkatreszFelhasznalasRequest r, CancellationToken ct)
    => Created($"/api/javitasok/{javitasId}/alkatreszek", await _service.Uj(javitasId, r, ct));

    [HttpDelete("{id:int}"), Authorize(Roles = Technikus)]
    public async Task<ActionResult> Torol(int javitasId, int id, CancellationToken ct)
    {
        await _service.Torol(javitasId, id, ct);
        return NoContent();
    }
}
