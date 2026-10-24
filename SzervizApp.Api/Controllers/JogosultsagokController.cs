using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzervizApp.DTOs;
using SzervizApp.Services;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Controllers;

[Route("api/jogosultsagok"), Authorize(Roles = Szerepkorok.Manager)]
public sealed class JogosultsagokController : SzervizControllerBase
{
    private readonly IJogosultsagService _service;

    public JogosultsagokController(IJogosultsagService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Jogosultsag>>> Lista(CancellationToken ct)
    => Ok(await _service.Lista(ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Jogosultsag>> Modosit(int id, JogosultsagRequest r, CancellationToken ct)
    => Ok(await _service.Modosit(id, r, ct));
}
