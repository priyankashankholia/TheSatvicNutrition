using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nutritionist.Api.Data;
using Nutritionist.Api.DTOs;

namespace Nutritionist.Api.Controllers;

[ApiController]
[Route("api/nutritionist/dashboard")]
public class NutritionistDashboardController : ControllerBase
{
    private readonly NutritionDbContext _context;

    public NutritionistDashboardController(NutritionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<NutritionistDashboardResponse>> GetDashboard()
    {
        var today = DateTime.UtcNow.Date;

        var totalClients = await _context.ClientProfiles
            .CountAsync();

        var upcomingAppointments = await _context.Appointments
            .CountAsync(x => x.StartTimeUtc >= DateTime.UtcNow);

        var pendingAssessments = await _context.Assessments
            .CountAsync();

        var unreadMessages = await _context.Messages
            .CountAsync();

        var activeClients = await _context.ClientProfiles
            .CountAsync();

        var response = new NutritionistDashboardResponse
        {
            TotalClients = totalClients,
            UpcomingAppointments = upcomingAppointments,
            PendingAssessments = pendingAssessments,
            UnreadMessages = unreadMessages,
            ActiveClients = activeClients
        };

        return Ok(response);
    }
}