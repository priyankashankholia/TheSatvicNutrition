namespace Nutritionist.Api.DTOs;

public class ClientDashboardResponse
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public double? CurrentWeightKg { get; set; }

    public double? HeightCm { get; set; }

    public double? Bmi { get; set; }

    public string? BmiCategory { get; set; }

    public string? ActivePackage { get; set; }

    public DateTime? PackageEndDateUtc { get; set; }

    public AppointmentSummary? NextAppointment { get; set; }

    public DietPlanSummary? ActiveDietPlan { get; set; }

    public int UnreadMessages { get; set; }

    public int UnreadNotifications { get; set; }
}

public class AppointmentSummary
{
    public Guid Id { get; set; }

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class DietPlanSummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DailyCalories { get; set; }

    public int DailyProteinGrams { get; set; }

    public DateTime EndDateUtc { get; set; }
}