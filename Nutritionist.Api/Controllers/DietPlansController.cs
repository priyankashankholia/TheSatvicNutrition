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
public class DietPlansController : ControllerBase
{
    private readonly NutritionDbContext _db;

    public DietPlansController(NutritionDbContext db)
    {
        _db = db;
    }

    [HttpGet("mine")]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<IActionResult> GetMyDietPlan()
    {
        var userId = GetUserId();

        var clientId = await _db.ClientProfiles
            .Where(x => x.UserId == userId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (clientId is null)
            return NotFound(new
            {
                message = "Client profile not found."
            });

        var plan = await _db.DietPlans
            .AsNoTracking()
            .Where(x =>
                x.ClientProfileId == clientId &&
                x.IsActive)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new DietPlanResponse
            {
                Id = x.Id,
                ClientProfileId = x.ClientProfileId,
                ClientName =
                    x.ClientProfile.User.FirstName + " " +
                    x.ClientProfile.User.LastName,
                Name = x.Name,
                Description = x.Description,
                DailyCalories = x.DailyCalories,
                DailyProteinGrams = x.DailyProteinGrams,
                StartDateUtc = x.StartDateUtc,
                EndDateUtc = x.EndDateUtc,
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .FirstOrDefaultAsync();

        if (plan is null)
            return NotFound(new
            {
                message = "No active diet plan found."
            });

        return Ok(plan);
    }

    [HttpGet("client/{clientId:guid}")]
    [Authorize(Roles = nameof(UserRole.Nutritionist))]
    public async Task<IActionResult> GetClientDietPlans(Guid clientId)
    {
        var exists = await _db.ClientProfiles
            .AnyAsync(x => x.Id == clientId);

        if (!exists)
            return NotFound(new
            {
                message = "Client not found."
            });

        var plans = await _db.DietPlans
            .AsNoTracking()
            .Where(x => x.ClientProfileId == clientId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new DietPlanResponse
            {
                Id = x.Id,
                ClientProfileId = x.ClientProfileId,
                ClientName =
                    x.ClientProfile.User.FirstName + " " +
                    x.ClientProfile.User.LastName,
                Name = x.Name,
                Description = x.Description,
                DailyCalories = x.DailyCalories,
                DailyProteinGrams = x.DailyProteinGrams,
                StartDateUtc = x.StartDateUtc,
                EndDateUtc = x.EndDateUtc,
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync();

        return Ok(plans);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Nutritionist))]
    public async Task<IActionResult> Create(
        CreateDietPlanRequest request)
    {
        var clientExists = await _db.ClientProfiles
            .AnyAsync(x => x.Id == request.ClientProfileId);

        if (!clientExists)
            return NotFound(new
            {
                message = "Client not found."
            });

        if (request.EndDateUtc <= request.StartDateUtc)
            return BadRequest(new
            {
                message = "End date must be after start date."
            });

        var existingPlans = await _db.DietPlans
            .Where(x =>
                x.ClientProfileId == request.ClientProfileId &&
                x.IsActive)
            .ToListAsync();

        foreach (var plan in existingPlans)
        {
            plan.IsActive = false;
            plan.UpdatedAtUtc = DateTime.UtcNow;
        }

        var userId = GetUserId();

        var dietPlan = new DietPlan
        {
            ClientProfileId = request.ClientProfileId,
            CreatedByUserId = userId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            DailyCalories = request.DailyCalories,
            DailyProteinGrams = request.DailyProteinGrams,
            StartDateUtc = request.StartDateUtc,
            EndDateUtc = request.EndDateUtc,
            IsActive = true
        };

        _db.DietPlans.Add(dietPlan);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            id = dietPlan.Id,
            message = "Diet plan created successfully."
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Nutritionist))]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateDietPlanRequest request)
    {
        var plan = await _db.DietPlans
            .FirstOrDefaultAsync(x => x.Id == id);

        if (plan is null)
            return NotFound(new
            {
                message = "Diet plan not found."
            });

        if (request.EndDateUtc <= request.StartDateUtc)
            return BadRequest(new
            {
                message = "End date must be after start date."
            });

        plan.Name = request.Name.Trim();
        plan.Description = request.Description.Trim();
        plan.DailyCalories = request.DailyCalories;
        plan.DailyProteinGrams = request.DailyProteinGrams;
        plan.StartDateUtc = request.StartDateUtc;
        plan.EndDateUtc = request.EndDateUtc;
        plan.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Diet plan updated successfully."
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

public class CreateDietPlanRequest
{
    public Guid ClientProfileId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DailyCalories { get; set; }

    public int DailyProteinGrams { get; set; }

    public DateTime StartDateUtc { get; set; }

    public DateTime EndDateUtc { get; set; }
}

public class UpdateDietPlanRequest
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DailyCalories { get; set; }

    public int DailyProteinGrams { get; set; }

    public DateTime StartDateUtc { get; set; }

    public DateTime EndDateUtc { get; set; }
}