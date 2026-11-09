using System.ComponentModel.DataAnnotations;

namespace ClinicDataApi.Models;

public sealed class SpravkaType
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    [Range(1, 1_000_000)]
    public int Price { get; set; }

    [Range(1, 120)]
    public int ValidMonths { get; set; } = 12;

    [MaxLength(300)]
    public string Description { get; set; } = "";

    public List<Spravka> Spravki { get; set; } = [];
}
