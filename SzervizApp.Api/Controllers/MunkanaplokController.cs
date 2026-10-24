using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzervizApp.DTOs;
using SzervizApp.Services;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Controllers;

[Route("api/javitasok/{javitasId:int}/munkanaplo")]
public sealed class MunkanaplokController : SzervizControllerBase
{
    private readonly IMunkanaploService _service;

    public MunkanaplokController(IMunkanaploService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MunkanaploDto>>> Lista(int javitasId, CancellationToken ct)
    => Ok(await _service.Lista(javitasId, ct));

    [HttpPost, Authorize(Roles = Technikus)]
    public async Task<ActionResult<MunkanaploDto>> Uj(int javitasId, MunkanaploRequest r, CancellationToken ct)
    => Created($"/api/javitasok/{javitasId}/munkanaplo", await _service.Uj(javitasId, r, ct));

    [HttpPut("{id:int}"), Authorize(Roles = Technikus)]
    public async Task<ActionResult<MunkanaploDto>> Modosit(int javitasId, int id, MunkanaploRequest r, CancellationToken ct)
    => Ok(await _service.Modosit(javitasId, id, r, ct));
}
