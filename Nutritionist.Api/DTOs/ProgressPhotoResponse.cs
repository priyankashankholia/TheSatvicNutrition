namespace Nutritionist.Api.DTOs;

public class ProgressPhotoResponse
{
    public Guid Id { get; set; }

    public string FileUrl { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime UploadedAtUtc { get; set; }
}