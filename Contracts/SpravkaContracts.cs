using System.ComponentModel.DataAnnotations;

namespace ClinicDataApi.Contracts;

public sealed class SpravkaRequest
{
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }

    [Range(1, int.MaxValue)]
    public int SpravkaTypeId { get; set; }

    public DateTime? IssuedAt { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = "";
}

public sealed record SpravkaResponse(
    int Id,
    int PatientId,
    string PatientName,
    int SpravkaTypeId,
    string SpravkaTypeName,
    int Price,
    DateTime IssuedAt,
    string Notes);

public sealed class SpravkaTypeRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";

    [Range(1, 1_000_000)]
    public int Price { get; set; }

    [Range(1, 120)]
    public int ValidMonths { get; set; }

    [MaxLength(300)]
    public string Description { get; set; } = "";
}

public sealed record SpravkaTypeResponse(int Id, string Name, int Price, int ValidMonths, string Description);
