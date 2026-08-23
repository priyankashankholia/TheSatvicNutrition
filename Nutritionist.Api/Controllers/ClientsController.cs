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
public class ClientsController : ControllerBase
{
    private readonly NutritionDbContext _db;

    public ClientsController(NutritionDbContext db)
    {
        _db = db;
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetUserId();

        var client = await _db.ClientProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (client is null)
        {
            return NotFound(new
            {
                message = "Client profile not found."
            });
        }

        var latestAssessment = await _db.Assessments
            .AsNoTracking()
            .Where(x => x.ClientProfileId == client.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        var activePurchase = await _db.Purchases
            .AsNoTracking()
            .Include(x => x.Package)
            .Where(x =>
                x.ClientProfileId == client.Id &&
                x.Status == PurchaseStatus.Paid &&
                x.EndDateUtc >= DateTime.UtcNow)
            .OrderByDescending(x => x.EndDateUtc)
            .FirstOrDefaultAsync();

        var nextAppointment = await _db.Appointments
            .AsNoTracking()
            .Where(x =>
                x.ClientProfileId == client.Id &&
                x.Status == AppointmentStatus.Scheduled &&
                x.StartTimeUtc >= DateTime.UtcNow)
            .OrderBy(x => x.StartTimeUtc)
            .FirstOrDefaultAsync();

        var activeDietPlan = await _db.DietPlans
            .AsNoTracking()
            .Where(x =>
                x.ClientProfileId == client.Id &&
                x.IsActive &&
                x.EndDateUtc >= DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        var unreadMessages = await _db.Messages
            .CountAsync(x =>
                x.ClientProfileId == client.Id &&
                !x.IsRead &&
                x.SenderUserId != userId);

        var unreadNotifications = await _db.Notifications
            .CountAsync(x =>
                x.UserId == userId &&
                !x.IsRead);

        var response = new ClientDashboardResponse
        {
            FirstName = client.User.FirstName,
            LastName = client.User.LastName,
            Email = client.User.Email,
            CurrentWeightKg = client.CurrentWeightKg,
            HeightCm = client.HeightCm,

            Bmi = latestAssessment?.Bmi,
            BmiCategory = latestAssessment?.BmiCategory,

            ActivePackage = activePurchase?.Package.Name,
            PackageEndDateUtc = activePurchase?.EndDateUtc,

            UnreadMessages = unreadMessages,
            UnreadNotifications = unreadNotifications
        };

        if (nextAppointment is not null)
        {
            response.NextAppointment = new AppointmentSummary
            {
                Id = nextAppointment.Id,
                StartTimeUtc = nextAppointment.StartTimeUtc,
                EndTimeUtc = nextAppointment.EndTimeUtc,
                Status = nextAppointment.Status.ToString()
            };
        }

        if (activeDietPlan is not null)
        {
            response.ActiveDietPlan = new DietPlanSummary
            {
                Id = activeDietPlan.Id,
                Name = activeDietPlan.Name,
                DailyCalories = activeDietPlan.DailyCalories,
                DailyProteinGrams = activeDietPlan.DailyProteinGrams,
                EndDateUtc = activeDietPlan.EndDateUtc
            };
        }

        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Nutritionist))]
    public async Task<IActionResult> GetClients()
    {
        var clients = await _db.ClientProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .OrderBy(x => x.User.FirstName)
            .Select(x => new
            {
                id = x.Id,
                userId = x.UserId,
                firstName = x.User.FirstName,
                lastName = x.User.LastName,
                email = x.User.Email,
                currentWeightKg = x.CurrentWeightKg,
                heightCm = x.HeightCm,
                createdAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(clients);
    }

    [HttpGet("{clientId:guid}")]
    [Authorize(Roles = nameof(UserRole.Nutritionist))]
    public async Task<IActionResult> GetClient(Guid clientId)
    {
        var client = await _db.ClientProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == clientId);

        if (client is null)
        {
            return NotFound(new
            {
                message = "Client not found."
            });
        }

        return Ok(new
        {
            id = client.Id,
            firstName = client.User.FirstName,
            lastName = client.User.LastName,
            email = client.User.Email,
            phoneNumber = client.PhoneNumber,
            dateOfBirth = client.DateOfBirth,
            gender = client.Gender,
            heightCm = client.HeightCm,
            currentWeightKg = client.CurrentWeightKg,
            dietaryPreferences = client.DietaryPreferences,
            allergies = client.Allergies,
            medicalNotes = client.MedicalNotes,
            createdAtUtc = client.CreatedAtUtc
        });
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        return userId;
    }
}