namespace Nutritionist.Api.Models;

public class Assessment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClientProfileId { get; set; }

    public ClientProfile ClientProfile { get; set; } = null!;

    public int Age { get; set; }

    public double WeightKg { get; set; }

    public double HeightCm { get; set; }

    public double Bmi { get; set; }

    public string BmiCategory { get; set; } = string.Empty;

    public string Goal { get; set; } = string.Empty;

    public string? ActivityLevel { get; set; }

    public string? DietaryPreference { get; set; }

    public string? Allergies { get; set; }

    public string? HealthNotes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}