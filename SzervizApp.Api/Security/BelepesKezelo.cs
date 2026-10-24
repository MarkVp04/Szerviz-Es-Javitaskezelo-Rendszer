using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Security;

public sealed class BelepesKezelo(AppDbContext db) : CookieAuthenticationEvents
{
    public static ClaimsPrincipal Principal(Felhasznalo f) => new(new ClaimsIdentity(
        new[] {
            new Claim(ClaimTypes.NameIdentifier, f.Id.ToString()),
            new Claim(ClaimTypes.Name, f.Felhasznalonev),
            new Claim(ClaimTypes.Role, f.Jogosultsag.Kod),
            new Claim("belepesVerzio", f.BelepesVerzio.ToString()),
            new Claim("ugyfelId", f.UgyfelId?.ToString() ?? "")
        }, CookieAuthenticationDefaults.AuthenticationScheme));

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var idText = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var f = int.TryParse(idText, out var id)
            ? await db.Felhasznalok.AsNoTracking().Include(x => x.Jogosultsag).Include(x => x.Ugyfel)
                .FirstOrDefaultAsync(x => x.Id == id, context.HttpContext.RequestAborted) : null;
        if (f is null || !f.Aktiv || f.BelepesVerzio.ToString() != context.Principal?.FindFirstValue("belepesVerzio")
            || f.Jogosultsag.Kod != context.Principal?.FindFirstValue(ClaimTypes.Role)
            || (f.Jogosultsag.Kod == Szerepkorok.Ugyfel && f.Ugyfel?.Aktiv != true))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
        }
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }
}
