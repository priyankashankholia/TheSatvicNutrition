using Nutritionist.Api.Models;

namespace Nutritionist.Api.Services;

public class NutritionAssessmentService
{
    public NutritionAssessmentResponse Assess(NutritionAssessmentRequest request)
    {
        if (request.WeightKg <= 0 || request.HeightCm <= 0)
            throw new ArgumentException("Weight and height must be greater than zero.");

        var heightMeters = request.HeightCm / 100m;

        var bmi = request.WeightKg /
                  (heightMeters * heightMeters);

        var category = bmi switch
        {
            < 18.5m => "Underweight",
            < 25m => "Normal",
            < 30m => "Overweight",
            _ => "Obese"
        };

        var recommendation = request.Goal switch
        {
            "Weight Loss" =>
                "Focus on controlled calories, high protein, vegetables and consistent activity.",

            "Muscle Gain" =>
                "Focus on adequate protein, progressive strength training and sufficient calories.",

            "Weight Maintenance" =>
                "Maintain balanced nutrition, adequate protein and regular physical activity.",

            _ =>
                "Focus on balanced nutrition, adequate protein, vegetables and regular activity."
        };

        return new NutritionAssessmentResponse
        {
            Bmi = Math.Round(bmi, 1),
            Category = category,
            Goal = request.Goal,
            Recommendation = recommendation
        };
    }
}
