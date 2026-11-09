using System.ComponentModel.DataAnnotations;

namespace ClinicDataApi.Models;

public sealed class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime StartsAt { get; set; }

    [Required, MaxLength(200)]
    public string Reason { get; set; } = "";

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Scheduled";

    public Patient Patient { get; set; } = null!;
}
