namespace Nutritionist.Api.DTOs;

public class DietPlanResponse
{
    public Guid Id { get; set; }

    public Guid ClientProfileId { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DailyCalories { get; set; }

    public int DailyProteinGrams { get; set; }

    public DateTime StartDateUtc { get; set; }

    public DateTime EndDateUtc { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}