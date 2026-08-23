namespace Nutritionist.Api.Models;

public class ProgressEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClientProfileId { get; set; }

    public ClientProfile ClientProfile { get; set; } = null!;

    public double WeightKg { get; set; }

    public double? WaistCm { get; set; }

    public double? HipCm { get; set; }

    public double? BodyFatPercentage { get; set; }

    public string? Notes { get; set; }

    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;
}