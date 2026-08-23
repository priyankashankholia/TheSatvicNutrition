namespace Nutritionist.Api.DTOs;

public class ClientProfileResponse
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public double? HeightCm { get; set; }

    public double? CurrentWeightKg { get; set; }

    public string? DietaryPreferences { get; set; }

    public string? Allergies { get; set; }

    public string? MedicalNotes { get; set; }
}