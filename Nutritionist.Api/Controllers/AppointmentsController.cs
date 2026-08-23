using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nutritionist.Api.Data;
using Nutritionist.Api.DTOs;
using Nutritionist.Api.Models;

namespace Nutritionist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly NutritionDbContext _db;

    public AppointmentsController(NutritionDbContext db)
    {
        _db = db;
    }

    [HttpGet("mine")]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<IActionResult> GetMyAppointments()
    {
        var userId = GetUserId();

        var client = await _db.ClientProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (client is null)
            return NotFound(new { message = "Client profile not found." });

        var appointments = await _db.Appointments
            .AsNoTracking()
            .Where(x => x.ClientProfileId == client.Id)
            .OrderBy(x => x.StartTimeUtc)
            .Select(x => new AppointmentResponse
            {
                Id = x.Id,
                ClientProfileId = x.ClientProfileId,
                ClientName =
                    x.ClientProfile.User.FirstName + " " +
                    x.ClientProfile.User.LastName,
                StartTimeUtc = x.StartTimeUtc,
                EndTimeUtc = x.EndTimeUtc,
                Status = x.Status.ToString(),
                ClientNotes = x.ClientNotes,
                NutritionistNotes = x.NutritionistNotes
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpGet("all")]
    [Authorize(Roles = nameof(UserRole.Nutritionist))]
    public async Task<IActionResult> GetAllAppointments()
    {
        var appointments = await _db.Appointments
            .AsNoTracking()
            .OrderBy(x => x.StartTimeUtc)
            .Select(x => new AppointmentResponse
            {
                Id = x.Id,
                ClientProfileId = x.ClientProfileId,
                ClientName =
                    x.ClientProfile.User.FirstName + " " +
                    x.ClientProfile.User.LastName,
                StartTimeUtc = x.StartTimeUtc,
                EndTimeUtc = x.EndTimeUtc,
                Status = x.Status.ToString(),
                ClientNotes = x.ClientNotes,
                NutritionistNotes = x.NutritionistNotes
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<IActionResult> Book(
        [FromBody] BookAppointmentRequest request)
    {
        var userId = GetUserId();

        var client = await _db.ClientProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (client is null)
            return NotFound(new { message = "Client profile not found." });

        var activePurchase = await _db.Purchases
            .Include(x => x.Package)
            .FirstOrDefaultAsync(x =>
                x.ClientProfileId == client.Id &&
                x.Status == PurchaseStatus.Paid &&
                x.StartDateUtc <= request.StartTimeUtc &&
                x.EndDateUtc >= request.StartTimeUtc);

        if (activePurchase is null)
        {
            return BadRequest(new
            {
                message =
                    "You need an active paid package to book an appointment."
            });
        }

        if (request.EndTimeUtc <= request.StartTimeUtc)
        {
            return BadRequest(new
            {
                message = "Appointment end time must be after start time."
            });
        }

        var conflict = await _db.Appointments.AnyAsync(x =>
            x.Status == AppointmentStatus.Scheduled &&
            x.StartTimeUtc < request.EndTimeUtc &&
            x.EndTimeUtc > request.StartTimeUtc);

        if (conflict)
        {
            return Conflict(new
            {
                message = "This time slot is already booked."
            });
        }

        var appointment = new Appointment
        {
            ClientProfileId = client.Id,
            PurchaseId = activePurchase.Id,
            StartTimeUtc = request.StartTimeUtc,
            EndTimeUtc = request.EndTimeUtc,
            ClientNotes = request.ClientNotes
        };

        _db.Appointments.Add(appointment);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            appointmentId = appointment.Id,
            message = "Appointment booked successfully."
        });
    }

    [HttpPut("{id:guid}/complete")]
    [Authorize(Roles = nameof(UserRole.Nutritionist))]
    public async Task<IActionResult> Complete(Guid id)
    {
        var appointment = await _db.Appointments
            .FirstOrDefaultAsync(x => x.Id == id);

        if (appointment is null)
            return NotFound(new { message = "Appointment not found." });

        appointment.Status = AppointmentStatus.Completed;
        appointment.CompletedAtUtc = DateTime.UtcNow;
        appointment.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Appointment marked as completed."
        });
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var appointment = await _db.Appointments
            .FirstOrDefaultAsync(x => x.Id == id);

        if (appointment is null)
            return NotFound(new { message = "Appointment not found." });

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Appointment cancelled."
        });
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException();

        return userId;
    }
}

public class BookAppointmentRequest
{
    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }

    public string? ClientNotes { get; set; }
}