namespace Nutritionist.Api.DTOs;

public class NutritionistProfileResponse
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Qualification { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public int ExperienceYears { get; set; }

    public string? Bio { get; set; }

    public string? ProfilePhotoUrl { get; set; }

    public bool IsAvailable { get; set; }
}