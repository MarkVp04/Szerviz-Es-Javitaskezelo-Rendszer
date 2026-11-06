using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzervizApp.DTOs;
using SzervizApp.Services;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Controllers;

[Route("api/javitasok")]
public sealed class JavitasokController : SzervizControllerBase
{
    private readonly IJavitasService _service;

    public JavitasokController(IJavitasService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JavitasDto>>> GetAll(CancellationToken ct, string? kereses = null,
        JavitasAllapot? allapot = null, int? felelosId = null, bool csakNyitott = false)
    => Ok(await _service.GetAll(ct, kereses, allapot, felelosId, csakNyitott));

    [HttpGet("feladatok"), Authorize(Roles = Munkatarsak)]
    public async Task<ActionResult<IEnumerable<FeladatDto>>> Feladatok(CancellationToken ct, bool sajat = false)
    => Ok(await _service.Feladatok(ct, sajat));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JavitasDto>> GetById(int id, CancellationToken ct)
    => Ok(await _service.GetById(id, ct));

    [HttpPost, Authorize(Roles = Recepcio)]
    public async Task<ActionResult<JavitasDto>> Create(UjJavitasRequest r, CancellationToken ct)
    {
        var result = await _service.Create(r, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}/felelos"), Authorize(Roles = Szerepkorok.Manager)]
    public async Task<ActionResult<JavitasDto>> Felelos(int id, FelelosRequest r, CancellationToken ct)
    => Ok(await _service.Felelos(id, r, ct));

    [HttpPost("{id:int}/vallalas"), Authorize(Roles = Szerepkorok.Szerelo)]
    public async Task<ActionResult<JavitasDto>> Vallalas(int id, CancellationToken ct)
    => Ok(await _service.Vallalas(id, ct));

    [HttpPut("{id:int}/allapot"), Authorize(Roles = Technikus)]
    public async Task<ActionResult<JavitasDto>> Allapot(int id, AllapotRequest r, CancellationToken ct)
    => Ok(await _service.Allapot(id, r, ct));

    [HttpPut("{id:int}/lezaras"), Authorize(Roles = Technikus)]
    public async Task<ActionResult<JavitasDto>> Lezaras(int id, JavitasLezarasRequest r, CancellationToken ct)
    => Ok(await _service.Lezaras(id, r, ct));

    [HttpGet("{id:int}/visszaadas")]
    public async Task<ActionResult<Visszaadas>> VisszaadasAdatok(int id, CancellationToken ct)
    => Ok(await _service.VisszaadasAdatok(id, ct));

    [HttpPost("{id:int}/visszaadas"), Authorize(Roles = Recepcio)]
    public async Task<ActionResult<VisszaadasDto>> Visszaadas(int id, VisszaadasRequest r, CancellationToken ct)
    {
        var result = await _service.Visszaadas(id, r, ct);
        return CreatedAtAction(nameof(VisszaadasAdatok), new { id }, result);
    }
}
