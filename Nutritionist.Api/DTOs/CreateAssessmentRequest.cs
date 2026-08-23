using System.ComponentModel.DataAnnotations;

namespace Nutritionist.Api.DTOs;

public class CreateAssessmentRequest
{
    [Required]
    public int Age { get; set; }

    [Required]
    [Range(1, 500)]
    public double WeightKg { get; set; }

    [Required]
    [Range(50, 250)]
    public double HeightCm { get; set; }

    [Required]
    public string Goal { get; set; } = string.Empty;

    public string? ActivityLevel { get; set; }

    public string? DietaryPreference { get; set; }

    public string? Allergies { get; set; }

    public string? HealthNotes { get; set; }
}