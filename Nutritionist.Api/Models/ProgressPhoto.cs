namespace Nutritionist.Api.Models;

public enum PhotoType
{
    Front = 1,
    Side = 2,
    Back = 3,
    Other = 4
}

public class ProgressPhoto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClientProfileId { get; set; }

    public ClientProfile ClientProfile { get; set; } = null!;

    public string FileUrl { get; set; } = string.Empty;

    public PhotoType Type { get; set; }

    public string? Notes { get; set; }

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}