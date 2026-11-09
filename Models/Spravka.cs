using System.ComponentModel.DataAnnotations;

namespace ClinicDataApi.Models;

public sealed class Spravka
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int SpravkaTypeId { get; set; }
    public DateTime IssuedAt { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = "";

    public Patient Patient { get; set; } = null!;
    public SpravkaType SpravkaType { get; set; } = null!;
}
