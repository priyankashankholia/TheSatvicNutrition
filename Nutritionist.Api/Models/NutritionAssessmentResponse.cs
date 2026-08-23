namespace Nutritionist.Api.Models;

public class NutritionAssessmentResponse
{
    public decimal Bmi { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}
