using System.ComponentModel.DataAnnotations;

namespace ClinicDataApi.Contracts;

public sealed class AppointmentRequest
{
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }

    public DateTime StartsAt { get; set; }

    [Required, StringLength(200, MinimumLength = 3)]
    public string Reason { get; set; } = "";

    [Required, StringLength(30, MinimumLength = 3)]
    public string Status { get; set; } = "Scheduled";
}

public sealed record AppointmentResponse(int Id, int PatientId, string PatientName, DateTime StartsAt, string Reason, string Status);
