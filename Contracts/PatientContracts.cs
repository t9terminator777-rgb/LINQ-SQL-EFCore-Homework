using System.ComponentModel.DataAnnotations;

namespace ClinicDataApi.Contracts;

public sealed class PatientRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";

    public DateTime BirthDate { get; set; }

    [Required, StringLength(25, MinimumLength = 5)]
    public string Phone { get; set; } = "";

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = "";

    [Required, StringLength(5, MinimumLength = 2)]
    public string BloodType { get; set; } = "";
}

public sealed record PatientResponse(int Id, string Name, DateTime BirthDate, string Phone, string Email, string BloodType);
