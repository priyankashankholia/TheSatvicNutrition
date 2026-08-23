namespace Nutritionist.Api.Models;

public class NutritionAssessmentRequest
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public decimal WeightKg { get; set; }
    public decimal HeightCm { get; set; }
    public string Goal { get; set; } = string.Empty;
}
