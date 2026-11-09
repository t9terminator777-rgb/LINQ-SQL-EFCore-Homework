using ClinicDataApi.Contracts;
using ClinicDataApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicDataApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LinqController(ClinicDbContext db) : ControllerBase
{
    [HttpGet("examples")]
    public async Task<IActionResult> GetExamples()
    {
        var expensiveSpravki = await db.Spravki.AsNoTracking()
            .Where(spravka => spravka.SpravkaType.Price > 150)
            .OrderByDescending(spravka => spravka.SpravkaType.Price)
            .Select(spravka => new SpravkaResponse(
                spravka.Id,
                spravka.PatientId,
                spravka.Patient.Name,
                spravka.SpravkaTypeId,
                spravka.SpravkaType.Name,
                spravka.SpravkaType.Price,
                spravka.IssuedAt,
                spravka.Notes))
            .ToListAsync();

        var totalRevenue = await db.Spravki.Select(spravka => (long?)spravka.SpravkaType.Price).SumAsync() ?? 0;
        var expensiveCount = await db.Spravki.CountAsync(spravka => spravka.SpravkaType.Price > 500);
        var hasAffordableSpravka = await db.Spravki.AnyAsync(spravka => spravka.SpravkaType.Price <= 200);
        var typeCounts = await db.Spravki.AsNoTracking()
            .GroupBy(spravka => new { spravka.SpravkaTypeId, spravka.SpravkaType.Name })
            .Select(group => new { group.Key.SpravkaTypeId, group.Key.Name, Count = group.Count() })
            .OrderByDescending(item => item.Count)
            .ToListAsync();

        return Ok(new
        {
            linqMethods = new[] { "Where", "Select", "OrderByDescending", "GroupBy", "Count", "Any", "Sum", "ToListAsync" },
            expensiveSpravki,
            totalRevenue,
            expensiveCount,
            hasAffordableSpravka,
            typeCounts
        });
    }

    [HttpGet("spravki-by-patient/{patientId:int}")]
    public async Task<ActionResult<IEnumerable<SpravkaResponse>>> GetSpravkiByPatient(int patientId)
    {
        if (!await db.Patients.AnyAsync(patient => patient.Id == patientId))
        {
            return NotFound(new { message = $"Пациент с id={patientId} не найден." });
        }

        var results = await db.Spravki.AsNoTracking()
            .Where(spravka => spravka.PatientId == patientId)
            .OrderBy(spravka => spravka.SpravkaType.Name)
            .Select(spravka => new SpravkaResponse(
                spravka.Id,
                spravka.PatientId,
                spravka.Patient.Name,
                spravka.SpravkaTypeId,
                spravka.SpravkaType.Name,
                spravka.SpravkaType.Price,
                spravka.IssuedAt,
                spravka.Notes))
            .ToListAsync();
        return Ok(results);
    }
}
