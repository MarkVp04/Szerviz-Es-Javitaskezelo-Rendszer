using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzervizApp.DTOs;
using SzervizApp.Services;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Controllers;

[Route("api/uzenetek"), Authorize(Roles = "Ugyfel,Recepcios,Manager")]
public sealed class UzenetekController : SzervizControllerBase
{
    private readonly IUzenetService _service;

    public UzenetekController(IUzenetService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UzenetListaDto>>> Lista(CancellationToken ct, int? ugyfelId = null, int? javitasId = null)
    => Ok(await _service.Lista(ct, ugyfelId, javitasId));

    [HttpPost]
    public async Task<ActionResult<UzenetDto>> Kuldes(UzenetRequest r, CancellationToken ct)
    => Created("/api/uzenetek", await _service.Kuldes(r, ct));
}
