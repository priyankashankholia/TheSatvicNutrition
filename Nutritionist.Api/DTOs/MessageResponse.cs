namespace Nutritionist.Api.DTOs;

public class MessageResponse
{
    public Guid Id { get; set; }

    public Guid ClientProfileId { get; set; }

    public Guid SenderUserId { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string SenderRole { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime SentAtUtc { get; set; }
}