using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nutritionist.Api.Data;
using Nutritionist.Api.DTOs;
using Nutritionist.Api.Models;

namespace Nutritionist.Api.Controllers;

[ApiController]
[Route("api/nutritionist-profile")]
[Authorize(Roles = nameof(UserRole.Nutritionist))]
public class NutritionistProfileController : ControllerBase
{
    private readonly NutritionDbContext _db;

    public NutritionistProfileController(NutritionDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<NutritionistProfileResponse>> GetProfile()
    {
        var userId = GetUserId();

        var profile = await _db.NutritionistProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
        {
            return NotFound(new
            {
                message = "Nutritionist profile not found."
            });
        }

        return Ok(MapProfile(profile));
    }

    [HttpPut]
    public async Task<ActionResult<NutritionistProfileResponse>> UpdateProfile(
        UpdateNutritionistProfileRequest request)
    {
        var userId = GetUserId();

        var profile = await _db.NutritionistProfiles
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
        {
            return NotFound(new
            {
                message = "Nutritionist profile not found."
            });
        }

        profile.User.FirstName = request.FirstName.Trim();
        profile.User.LastName = request.LastName.Trim();

        profile.Qualification =
            request.Qualification.Trim();

        profile.Specialization =
            request.Specialization.Trim();

        profile.ExperienceYears =
            request.ExperienceYears;

        profile.Bio = request.Bio?.Trim();
        profile.PhoneNumber = request.PhoneNumber?.Trim();
        profile.ProfilePhotoUrl =
            request.ProfilePhotoUrl?.Trim();

        profile.IsAvailable = request.IsAvailable;
        profile.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(MapProfile(profile));
    }

    private static NutritionistProfileResponse MapProfile(
        NutritionistProfile profile)
    {
        return new NutritionistProfileResponse
        {
            Id = profile.Id,
            FirstName = profile.User.FirstName,
            LastName = profile.User.LastName,
            Email = profile.User.Email,
            PhoneNumber = profile.PhoneNumber,
            Qualification = profile.Qualification,
            Specialization = profile.Specialization,
            ExperienceYears = profile.ExperienceYears,
            Bio = profile.Bio,
            ProfilePhotoUrl = profile.ProfilePhotoUrl,
            IsAvailable = profile.IsAvailable
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

public class UpdateNutritionistProfileRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Qualification { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public int ExperienceYears { get; set; }

    public string? Bio { get; set; }

    public string? ProfilePhotoUrl { get; set; }

    public bool IsAvailable { get; set; } = true;
}