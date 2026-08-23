namespace Nutritionist.Api.DTOs;

public class PackageResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DurationWeeks { get; set; }

    public int AppointmentsPerWeek { get; set; }

    public bool IncludesDietPlan { get; set; }

    public bool IncludesProgressTracking { get; set; }

    public bool IncludesMessaging { get; set; }

    public bool IncludesPhotoTracking { get; set; }
}