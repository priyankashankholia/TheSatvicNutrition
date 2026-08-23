namespace Nutritionist.Api.DTOs;

public class AppointmentResponse
{
    public Guid Id { get; set; }

    public Guid ClientProfileId { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? ClientNotes { get; set; }

    public string? NutritionistNotes { get; set; }
}