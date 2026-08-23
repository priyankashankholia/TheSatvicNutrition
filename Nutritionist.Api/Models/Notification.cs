namespace Nutritionist.Api.Models;

public enum NotificationType
{
    Appointment = 1,
    Purchase = 2,
    Message = 3,
    DietPlan = 4,
    Progress = 5,
    General = 6
}

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAtUtc { get; set; }
}