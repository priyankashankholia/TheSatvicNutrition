namespace Nutritionist.Api.Models;

public class Package
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DurationWeeks { get; set; }

    public int AppointmentsPerWeek { get; set; } = 1;

    public bool IncludesDietPlan { get; set; } = true;

    public bool IncludesProgressTracking { get; set; } = true;

    public bool IncludesMessaging { get; set; } = true;

    public bool IncludesPhotoTracking { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Purchase> Purchases { get; set; } = [];
}