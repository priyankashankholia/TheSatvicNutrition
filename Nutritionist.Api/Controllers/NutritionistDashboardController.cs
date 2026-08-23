using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nutritionist.Api.DTOs;

namespace Nutritionist.Api.Controllers;

[ApiController]
[Route("api/nutritionist/dashboard")]
[Authorize(Roles = "Nutritionist")]
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

        var totalClients = await _context.Users
            .CountAsync(x => x.Role == "Client");

        var todayAppointments = await _context.Appointments
            .CountAsync(x => x.AppointmentDate.Date == today);

        var pendingAssessments = await _context.Assessments
            .CountAsync(x => x.Status == "Pending");

        var unreadMessages = await _context.Messages
            .CountAsync(x => !x.IsRead);

        var appointments = await _context.Appointments
            .Where(x => x.AppointmentDate.Date == today)
            .OrderBy(x => x.AppointmentDate)
            .Select(x => new NutritionistAppointmentResponse
            {
                Id = x.Id,
                ClientName = x.Client.Name,
                Date = x.AppointmentDate,
                Time = x.AppointmentDate.ToString("hh:mm tt"),
                Status = x.Status
            })
            .ToListAsync();

        var response = new NutritionistDashboardResponse
        {
            TotalClients = totalClients,
            TodayAppointments = todayAppointments,
            PendingAssessments = pendingAssessments,
            UnreadMessages = unreadMessages,
            Appointments = appointments
        };

        return Ok(response);
    }
}