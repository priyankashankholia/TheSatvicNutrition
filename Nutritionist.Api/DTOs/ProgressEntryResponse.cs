namespace Nutritionist.Api.DTOs;

public class ProgressEntryResponse
{
    public Guid Id { get; set; }

    public double WeightKg { get; set; }

    public double? WaistCm { get; set; }

    public double? HipCm { get; set; }

    public double? BodyFatPercentage { get; set; }

    public string? Notes { get; set; }

    public DateTime RecordedAtUtc { get; set; }
}