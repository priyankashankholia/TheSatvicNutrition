namespace Nutritionist.Api.Models;

public class ClientProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public double? HeightCm { get; set; }

    public double? CurrentWeightKg { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? MedicalNotes { get; set; }

    public string? DietaryPreferences { get; set; }

    public string? Allergies { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Assessment> Assessments { get; set; } = [];

    public ICollection<Purchase> Purchases { get; set; } = [];

    public ICollection<Appointment> Appointments { get; set; } = [];

    public ICollection<ProgressEntry> ProgressEntries { get; set; } = [];

    public ICollection<ProgressPhoto> ProgressPhotos { get; set; } = [];

    public ICollection<Message> Messages { get; set; } = [];
}