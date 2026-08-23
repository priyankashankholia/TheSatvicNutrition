namespace Nutritionist.Api.Models;

public enum AppointmentStatus
{
    Scheduled = 1,
    Completed = 2,
    Cancelled = 3,
    NoShow = 4
}

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClientProfileId { get; set; }

    public ClientProfile ClientProfile { get; set; } = null!;

    public Guid PurchaseId { get; set; }

    public Purchase Purchase { get; set; } = null!;

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }

    public AppointmentStatus Status { get; set; } =
        AppointmentStatus.Scheduled;

    public string? ClientNotes { get; set; }

    public string? NutritionistNotes { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } =
        DateTime.UtcNow;
}