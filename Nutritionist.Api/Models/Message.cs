namespace Nutritionist.Api.Models;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClientProfileId { get; set; }

    public ClientProfile ClientProfile { get; set; } = null!;

    public Guid SenderUserId { get; set; }

    public User SenderUser { get; set; } = null!;

    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAtUtc { get; set; }
}