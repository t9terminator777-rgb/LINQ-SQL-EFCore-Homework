using System.ComponentModel.DataAnnotations;

namespace ClinicDataApi.Models;

public sealed class Patient
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    public DateTime BirthDate { get; set; }

    [Required, MaxLength(25)]
    public string Phone { get; set; } = "";

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = "";

    [Required, MaxLength(5)]
    public string BloodType { get; set; } = "";

    public List<Spravka> Spravki { get; set; } = [];
    public List<Appointment> Appointments { get; set; } = [];
}
