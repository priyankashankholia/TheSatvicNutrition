namespace Nutritionist.Api.DTOs;

public class NutritionistDashboardResponse
{
    public int TotalClients { get; set; }

    public int UpcomingAppointments { get; set; }

    public int PendingAssessments { get; set; }

    public int UnreadMessages { get; set; }

    public int ActiveClients { get; set; }
}