using ClinicDataApi.Contracts;
using ClinicDataApi.Data;
using ClinicDataApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicDataApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SpravkiController(ClinicDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpravkaResponse>>> GetAll(
        [FromQuery] int? patientId,
        [FromQuery] string? search,
        [FromQuery] bool priceDescending = false)
    {
        IQueryable<Spravka> query = db.Spravki.AsNoTracking();
        if (patientId.HasValue)
        {
            query = query.Where(spravka => spravka.PatientId == patientId.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(spravka => spravka.SpravkaType.Name.Contains(term) || spravka.Patient.Name.Contains(term));
        }

        query = priceDescending
            ? query.OrderByDescending(spravka => spravka.SpravkaType.Price)
            : query.OrderBy(spravka => spravka.SpravkaType.Price);

        return Ok(await query.Select(ToResponseExpression()).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SpravkaResponse>> GetById(int id)
    {
        var response = await db.Spravki.AsNoTracking()
            .Where(spravka => spravka.Id == id)
            .Select(ToResponseExpression())
            .FirstOrDefaultAsync();
        return response is null ? NotFound(new { message = $"Справка с id={id} не найдена." }) : Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<SpravkaResponse>> Create(SpravkaRequest request)
    {
        var error = await ValidateReferences(request);
        if (error is not null) return BadRequest(new { message = error });

        var issuedAt = request.IssuedAt ?? DateTime.UtcNow;
        if (issuedAt > DateTime.UtcNow.AddMinutes(5))
        {
            return BadRequest(new { message = "Дата выдачи справки не может быть в будущем." });
        }

        var spravka = new Spravka
        {
            PatientId = request.PatientId,
            SpravkaTypeId = request.SpravkaTypeId,
            IssuedAt = issuedAt,
            Notes = request.Notes.Trim()
        };
        db.Spravki.Add(spravka);
        await db.SaveChangesAsync();
        var response = await db.Spravki.AsNoTracking().Where(item => item.Id == spravka.Id)
            .Select(ToResponseExpression()).SingleAsync();
        return CreatedAtAction(nameof(GetById), new { id = spravka.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SpravkaResponse>> Update(int id, SpravkaRequest request)
    {
        var spravka = await db.Spravki.FindAsync(id);
        if (spravka is null)
        {
            return NotFound(new { message = $"Справка с id={id} не найдена." });
        }

        var error = await ValidateReferences(request);
        if (error is not null) return BadRequest(new { message = error });
        var issuedAt = request.IssuedAt ?? spravka.IssuedAt;
        if (issuedAt > DateTime.UtcNow.AddMinutes(5))
        {
            return BadRequest(new { message = "Дата выдачи справки не может быть в будущем." });
        }

        spravka.PatientId = request.PatientId;
        spravka.SpravkaTypeId = request.SpravkaTypeId;
        spravka.IssuedAt = issuedAt;
        spravka.Notes = request.Notes.Trim();
        await db.SaveChangesAsync();
        var response = await db.Spravki.AsNoTracking().Where(item => item.Id == id)
            .Select(ToResponseExpression()).SingleAsync();
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var spravka = await db.Spravki.FindAsync(id);
        if (spravka is null)
        {
            return NotFound(new { message = $"Справка с id={id} не найдена." });
        }

        db.Spravki.Remove(spravka);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateReferences(SpravkaRequest request)
    {
        if (!await db.Patients.AnyAsync(patient => patient.Id == request.PatientId))
        {
            return $"Пациент с id={request.PatientId} не существует.";
        }
        if (!await db.SpravkaTypes.AnyAsync(type => type.Id == request.SpravkaTypeId))
        {
            return $"Вид справки с id={request.SpravkaTypeId} не существует.";
        }
        return null;
    }

    private static System.Linq.Expressions.Expression<Func<Spravka, SpravkaResponse>> ToResponseExpression() =>
        spravka => new SpravkaResponse(
            spravka.Id,
            spravka.PatientId,
            spravka.Patient.Name,
            spravka.SpravkaTypeId,
            spravka.SpravkaType.Name,
            spravka.SpravkaType.Price,
            spravka.IssuedAt,
            spravka.Notes);
}
