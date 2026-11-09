using ClinicDataApi.Contracts;
using ClinicDataApi.Data;
using ClinicDataApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicDataApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PatientsController(ClinicDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientResponse>>> GetAll([FromQuery] string? search)
    {
        var query = db.Patients.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(patient => patient.Name.Contains(search.Trim()) || patient.Email.Contains(search.Trim()));
        }

        var patients = await query.OrderBy(patient => patient.Name)
            .Select(patient => new PatientResponse(patient.Id, patient.Name, patient.BirthDate, patient.Phone, patient.Email, patient.BloodType))
            .ToListAsync();
        return Ok(patients);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PatientResponse>> GetById(int id)
    {
        var patient = await db.Patients.AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new PatientResponse(item.Id, item.Name, item.BirthDate, item.Phone, item.Email, item.BloodType))
            .FirstOrDefaultAsync();
        return patient is null ? NotFound(new { message = $"Пациент с id={id} не найден." }) : Ok(patient);
    }

    [HttpGet("{id:int}/spravki")]
    public async Task<ActionResult<IEnumerable<SpravkaResponse>>> GetPatientSpravki(int id)
    {
        if (!await db.Patients.AnyAsync(patient => patient.Id == id))
        {
            return NotFound(new { message = $"Пациент с id={id} не найден." });
        }

        var spravki = await db.Spravki.AsNoTracking()
            .Where(spravka => spravka.PatientId == id)
            .OrderByDescending(spravka => spravka.IssuedAt)
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
        return Ok(spravki);
    }

    [HttpPost]
    public async Task<ActionResult<PatientResponse>> Create(PatientRequest request)
    {
        if (request.BirthDate.Date > DateTime.UtcNow.Date || request.BirthDate == default)
        {
            return BadRequest(new { message = "Дата рождения должна быть заполнена и не может быть в будущем." });
        }

        var email = request.Email.Trim();
        if (await db.Patients.AnyAsync(patient => patient.Email == email))
        {
            return Conflict(new { message = "Пациент с таким email уже зарегистрирован." });
        }

        var patient = new Patient
        {
            Name = request.Name.Trim(),
            BirthDate = request.BirthDate,
            Phone = request.Phone.Trim(),
            Email = email,
            BloodType = request.BloodType.Trim().ToUpperInvariant()
        };
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var response = ToResponse(patient);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PatientResponse>> Update(int id, PatientRequest request)
    {
        var patient = await db.Patients.FindAsync(id);
        if (patient is null)
        {
            return NotFound(new { message = $"Пациент с id={id} не найден." });
        }
        if (request.BirthDate.Date > DateTime.UtcNow.Date || request.BirthDate == default)
        {
            return BadRequest(new { message = "Дата рождения должна быть заполнена и не может быть в будущем." });
        }

        var email = request.Email.Trim();
        if (await db.Patients.AnyAsync(other => other.Id != id && other.Email == email))
        {
            return Conflict(new { message = "Этот email уже используется другим пациентом." });
        }

        patient.Name = request.Name.Trim();
        patient.BirthDate = request.BirthDate;
        patient.Phone = request.Phone.Trim();
        patient.Email = email;
        patient.BloodType = request.BloodType.Trim().ToUpperInvariant();
        await db.SaveChangesAsync();
        return Ok(ToResponse(patient));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await db.Patients.FindAsync(id);
        if (patient is null)
        {
            return NotFound(new { message = $"Пациент с id={id} не найден." });
        }

        db.Patients.Remove(patient);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static PatientResponse ToResponse(Patient patient) =>
        new(patient.Id, patient.Name, patient.BirthDate, patient.Phone, patient.Email, patient.BloodType);
}
