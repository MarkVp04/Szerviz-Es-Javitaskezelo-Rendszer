using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzervizApp.Api.Contracts;
using SzervizApp.DataContext.Context;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.Api.Controllers;

[ApiController]
[Route("api/javitasok")]
public class JavitasokController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public JavitasokController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET: /api/javitasok
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<JavitasDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var javitasok = await _dbContext.Javitasok
            .AsNoTracking()
            .OrderByDescending(j => j.AtvetelDatuma)
            .ToListAsync(cancellationToken);

        return Ok(javitasok.Select(ToDto).ToList());
    }

    // GET: /api/javitasok/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<JavitasDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var javitas = await _dbContext.Javitasok
            .AsNoTracking()
            .FirstOrDefaultAsync(
                j => j.Id == id,
                cancellationToken);

        if (javitas is null)
        {
            return NotFound(new
            {
                message = $"A(z) {id} azonosítójú javítás nem található."
            });
        }

        return Ok(ToDto(javitas));
    }

    // POST: /api/javitasok
    [HttpPost]
    public async Task<ActionResult<JavitasDto>> Create(
        [FromBody] UjJavitasRequest request,
        CancellationToken cancellationToken)
    {
        var ugyfelLetezik = await _dbContext.Ugyfelek
            .AnyAsync(
                u => u.Id == request.UgyfelId,
                cancellationToken);

        if (!ugyfelLetezik)
        {
            return NotFound(new
            {
                message = "A megadott ügyfél nem található."
            });
        }

        var eszkozLetezik = await _dbContext.Eszkozok
            .AnyAsync(
                e => e.Id == request.EszkozId,
                cancellationToken);

        if (!eszkozLetezik)
        {
            return NotFound(new
            {
                message = "A megadott eszköz nem található."
            });
        }

        var javitas = new Javitas
        {
            UgyfelId = request.UgyfelId,
            EszkozId = request.EszkozId,
            BejelentettHiba = request.BejelentettHiba.Trim(),

            // Ezeket nem a kliens határozza meg.
            AtvetelDatuma = DateTime.UtcNow,
            Allapot = JavitasAllapot.Atvett
        };

        _dbContext.Javitasok.Add(javitas);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = javitas.Id },
            ToDto(javitas));
    }

    // PUT: /api/javitasok/1/lezaras
    [HttpPut("{id:int}/lezaras")]
    public async Task<ActionResult<JavitasDto>> Lezaras(
        int id,
        [FromBody] JavitasLezarasRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(
                typeof(JavitasEredmeny),
                request.Eredmeny))
        {
            return BadRequest(new
            {
                message = "Érvénytelen javítási eredmény."
            });
        }

        // Sikertelen vagy javítás nélkül lezárt ügy esetén
        // kötelező az indoklás.
        if (request.Eredmeny != JavitasEredmeny.Javitva
            && string.IsNullOrWhiteSpace(request.LezarasIndoklasa))
        {
            return BadRequest(new
            {
                message = "Ehhez a lezárási eredményhez indoklás szükséges."
            });
        }

        var javitas = await _dbContext.Javitasok
            .FirstOrDefaultAsync(
                j => j.Id == id,
                cancellationToken);

        if (javitas is null)
        {
            return NotFound(new
            {
                message = $"A(z) {id} azonosítójú javítás nem található."
            });
        }

        if (javitas.Allapot == JavitasAllapot.Lezart)
        {
            return Conflict(new
            {
                message = "Ez a javítás már le van zárva."
            });
        }

        javitas.Allapot = JavitasAllapot.Lezart;
        javitas.LezarasDatuma = DateTime.UtcNow;
        javitas.Eredmeny = request.Eredmeny;
        javitas.LezarasIndoklasa =
            string.IsNullOrWhiteSpace(request.LezarasIndoklasa)
                ? null
                : request.LezarasIndoklasa.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(javitas));
    }

    private static JavitasDto ToDto(Javitas javitas)
    {
        return new JavitasDto(
            javitas.Id,
            javitas.UgyfelId,
            javitas.EszkozId,
            javitas.BejelentettHiba,
            javitas.AtvetelDatuma,
            javitas.Allapot,
            javitas.LezarasDatuma,
            javitas.Eredmeny,
            javitas.LezarasIndoklasa);
    }
}