using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SzervizApp.DTOs;
using SzervizApp.Services;

namespace SzervizApp.Api.Controllers;

[Route("api/auth")]
public sealed class AuthController : SzervizControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous, HttpPost("bejelentkezes")]
    public async Task<ActionResult<LoginResponseDto>> Belepes(BelepesRequest request, CancellationToken ct)
        => Ok(await _authService.Belepes(request, ct));

    [HttpPost("kijelentkezes")]
    public async Task<ActionResult> Kilepes(CancellationToken ct)
    {
        await _authService.Kilepes(ct);
        return NoContent();
    }

    [HttpGet("en")]
    public async Task<ActionResult<FelhasznaloDto>> En(CancellationToken ct)
        => Ok(await _authService.En(ct));

    [HttpPut("jelszo")]
    public async Task<ActionResult<LoginResponseDto>> Jelszo(JelszoRequest request, CancellationToken ct)
        => Ok(await _authService.Jelszo(request, ct));
}
