using ClinicDataApi.Contracts;
using ClinicDataApi.Data;
using ClinicDataApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicDataApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AppointmentsController(ClinicDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAll([FromQuery] int? patientId, [FromQuery] string? status)
    {
        var query = db.Appointments.AsNoTracking();
        if (patientId.HasValue) query = query.Where(item => item.PatientId == patientId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(item => item.Status == status.Trim());

        var appointments = await query.OrderBy(item => item.StartsAt)
            .Select(item => new AppointmentResponse(item.Id, item.PatientId, item.Patient.Name, item.StartsAt, item.Reason, item.Status))
            .ToListAsync();
        return Ok(appointments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentResponse>> GetById(int id)
    {
        var appointment = await db.Appointments.AsNoTracking().Where(item => item.Id == id)
            .Select(item => new AppointmentResponse(item.Id, item.PatientId, item.Patient.Name, item.StartsAt, item.Reason, item.Status))
            .FirstOrDefaultAsync();
        return appointment is null ? NotFound(new { message = $"Запись на приём с id={id} не найдена." }) : Ok(appointment);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> Create(AppointmentRequest request)
    {
        var validation = await ValidateRequest(request);
        if (validation is not null) return BadRequest(new { message = validation });

        var appointment = new Appointment
        {
            PatientId = request.PatientId,
            StartsAt = request.StartsAt,
            Reason = request.Reason.Trim(),
            Status = request.Status.Trim()
        };
        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();
        var response = await db.Appointments.AsNoTracking().Where(item => item.Id == appointment.Id)
            .Select(item => new AppointmentResponse(item.Id, item.PatientId, item.Patient.Name, item.StartsAt, item.Reason, item.Status))
            .SingleAsync();
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AppointmentResponse>> Update(int id, AppointmentRequest request)
    {
        var appointment = await db.Appointments.FindAsync(id);
        if (appointment is null) return NotFound(new { message = $"Запись на приём с id={id} не найдена." });

        var validation = await ValidateRequest(request);
        if (validation is not null) return BadRequest(new { message = validation });
        appointment.PatientId = request.PatientId;
        appointment.StartsAt = request.StartsAt;
        appointment.Reason = request.Reason.Trim();
        appointment.Status = request.Status.Trim();
        await db.SaveChangesAsync();

        var response = await db.Appointments.AsNoTracking().Where(item => item.Id == id)
            .Select(item => new AppointmentResponse(item.Id, item.PatientId, item.Patient.Name, item.StartsAt, item.Reason, item.Status))
            .SingleAsync();
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var appointment = await db.Appointments.FindAsync(id);
        if (appointment is null) return NotFound(new { message = $"Запись на приём с id={id} не найдена." });
        db.Appointments.Remove(appointment);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateRequest(AppointmentRequest request)
    {
        if (!await db.Patients.AnyAsync(patient => patient.Id == request.PatientId))
            return $"Пациент с id={request.PatientId} не существует.";
        if (request.StartsAt <= DateTime.UtcNow)
            return "Дата и время приёма должны быть в будущем.";
        if (request.Status.Trim() is not ("Scheduled" or "Completed" or "Cancelled"))
            return "Статус должен быть Scheduled, Completed или Cancelled.";
        return null;
    }
}
