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
public class AssessmentController : ControllerBase
{
    private readonly NutritionDbContext _db;

    public AssessmentController(NutritionDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<IActionResult> Create(
        CreateAssessmentRequest request)
    {
        var userId = GetUserId();

        var client = await _db.ClientProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (client is null)
        {
            return NotFound(new
            {
                message = "Client profile not found."
            });
        }

        var bmi = CalculateBmi(
            request.WeightKg,
            request.HeightCm);

        var assessment = new Assessment
        {
            ClientProfileId = client.Id,
            Age = request.Age,
            WeightKg = request.WeightKg,
            HeightCm = request.HeightCm,
            Bmi = Math.Round(bmi, 2),
            BmiCategory = GetBmiCategory(bmi),
            Goal = request.Goal.Trim(),
            ActivityLevel = request.ActivityLevel,
            DietaryPreference = request.DietaryPreference,
            Allergies = request.Allergies,
            HealthNotes = request.HealthNotes
        };

        client.CurrentWeightKg = request.WeightKg;
        client.HeightCm = request.HeightCm;
        client.UpdatedAtUtc = DateTime.UtcNow;

        _db.Assessments.Add(assessment);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            id = assessment.Id,
            bmi = assessment.Bmi,
            bmiCategory = assessment.BmiCategory,
            message = "Assessment submitted successfully."
        });
    }

    [HttpGet("mine")]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<IActionResult> GetMine()
    {
        var userId = GetUserId();

        var clientId = await _db.ClientProfiles
            .Where(x => x.UserId == userId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (clientId is null)
        {
            return NotFound(new
            {
                message = "Client profile not found."
            });
        }

        var assessments = await _db.Assessments
            .AsNoTracking()
            .Where(x => x.ClientProfileId == clientId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return Ok(assessments);
    }

    [HttpGet("client/{clientId:guid}")]
    [Authorize(Roles = nameof(UserRole.Nutritionist))]
    public async Task<IActionResult> GetClientAssessments(
        Guid clientId)
    {
        var assessments = await _db.Assessments
            .AsNoTracking()
            .Where(x => x.ClientProfileId == clientId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return Ok(assessments);
    }

    private static double CalculateBmi(
        double weightKg,
        double heightCm)
    {
        var heightMeters = heightCm / 100;

        return weightKg /
               (heightMeters * heightMeters);
    }

    private static string GetBmiCategory(double bmi)
    {
        return bmi switch
        {
            < 18.5 => "Underweight",
            < 25 => "Normal",
            < 30 => "Overweight",
            _ => "Obese"
        };
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