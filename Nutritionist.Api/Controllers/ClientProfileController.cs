using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nutritionist.Api.Data;
using Nutritionist.Api.DTOs;
using Nutritionist.Api.Models;

namespace Nutritionist.Api.Controllers;

[ApiController]
[Route("api/client-profile")]
[Authorize(Roles = nameof(UserRole.Client))]
public class ClientProfileController : ControllerBase
{
    private readonly NutritionDbContext _db;

    public ClientProfileController(NutritionDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ClientProfileResponse>> GetProfile()
    {
        var userId = GetUserId();

        var profile = await _db.ClientProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
        {
            return NotFound(new
            {
                message = "Client profile not found."
            });
        }

        return Ok(MapProfile(profile));
    }

    [HttpPut]
    public async Task<ActionResult<ClientProfileResponse>> UpdateProfile(
        UpdateClientProfileRequest request)
    {
        var userId = GetUserId();

        var profile = await _db.ClientProfiles
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
        {
            return NotFound(new
            {
                message = "Client profile not found."
            });
        }

        profile.User.FirstName = request.FirstName.Trim();
        profile.User.LastName = request.LastName.Trim();

        profile.DateOfBirth = request.DateOfBirth;
        profile.Gender = request.Gender?.Trim();
        profile.PhoneNumber = request.PhoneNumber?.Trim();
        profile.HeightCm = request.HeightCm;
        profile.DietaryPreferences =
            request.DietaryPreferences?.Trim();
        profile.Allergies = request.Allergies?.Trim();
        profile.MedicalNotes = request.MedicalNotes?.Trim();
        profile.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(MapProfile(profile));
    }

    private static ClientProfileResponse MapProfile(
        ClientProfile profile)
    {
        return new ClientProfileResponse
        {
            Id = profile.Id,
            FirstName = profile.User.FirstName,
            LastName = profile.User.LastName,
            Email = profile.User.Email,
            PhoneNumber = profile.PhoneNumber,
            DateOfBirth = profile.DateOfBirth,
            Gender = profile.Gender,
            HeightCm = profile.HeightCm,
            CurrentWeightKg = profile.CurrentWeightKg,
            DietaryPreferences = profile.DietaryPreferences,
            Allergies = profile.Allergies,
            MedicalNotes = profile.MedicalNotes
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

public class UpdateClientProfileRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public double? HeightCm { get; set; }

    public string? DietaryPreferences { get; set; }

    public string? Allergies { get; set; }

    public string? MedicalNotes { get; set; }
}