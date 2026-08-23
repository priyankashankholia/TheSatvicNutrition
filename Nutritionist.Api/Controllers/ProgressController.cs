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
public class ProgressController : ControllerBase
{
    private readonly NutritionDbContext _db;

    public ProgressController(NutritionDbContext db)
    {
        _db = db;
    }

    [HttpGet("mine")]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<IActionResult> GetMyProgress()
    {
        var userId = GetUserId();

        var clientId = await _db.ClientProfiles
            .Where(x => x.UserId == userId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (clientId is null)
            return NotFound(new { message = "Client profile not found." });

        var progress = await _db.ProgressEntries
            .AsNoTracking()
            .Where(x => x.ClientProfileId == clientId)
            .OrderBy(x => x.RecordedAtUtc)
            .Select(x => new ProgressEntryResponse
            {
                Id = x.Id,
                WeightKg = x.WeightKg,
                WaistCm = x.WaistCm,
                HipCm = x.HipCm,
                BodyFatPercentage = x.BodyFatPercentage,
                Notes = x.Notes,
                RecordedAtUtc = x.RecordedAtUtc
            })
            .ToListAsync();

        return Ok(progress);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<IActionResult> AddProgress(
        [FromBody] AddProgressRequest request)
    {
        var userId = GetUserId();

        var client = await _db.ClientProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (client is null)
            return NotFound(new { message = "Client profile not found." });

        if (request.WeightKg <= 0)
            return BadRequest(new { message = "Weight must be greater than zero." });

        var entry = new ProgressEntry
        {
            ClientProfileId = client.Id,
            WeightKg = request.WeightKg,
            WaistCm = request.WaistCm,
            HipCm = request.HipCm,
            BodyFatPercentage = request.BodyFatPercentage,
            Notes = request.Notes,
            RecordedAtUtc = DateTime.UtcNow
        };

        _db.ProgressEntries.Add(entry);

        client.CurrentWeightKg = request.WeightKg;
        client.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            id = entry.Id,
            message = "Progress recorded successfully."
        });
    }

    [HttpGet("client/{clientId:guid}")]
    [Authorize(Roles = nameof(UserRole.Nutritionist))]
    public async Task<IActionResult> GetClientProgress(Guid clientId)
    {
        var exists = await _db.ClientProfiles
            .AnyAsync(x => x.Id == clientId);

        if (!exists)
            return NotFound(new { message = "Client not found." });

        var progress = await _db.ProgressEntries
            .AsNoTracking()
            .Where(x => x.ClientProfileId == clientId)
            .OrderBy(x => x.RecordedAtUtc)
            .Select(x => new ProgressEntryResponse
            {
                Id = x.Id,
                WeightKg = x.WeightKg,
                WaistCm = x.WaistCm,
                HipCm = x.HipCm,
                BodyFatPercentage = x.BodyFatPercentage,
                Notes = x.Notes,
                RecordedAtUtc = x.RecordedAtUtc
            })
            .ToListAsync();

        return Ok(progress);
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException();

        return userId;
    }
}

public class AddProgressRequest
{
    public double WeightKg { get; set; }

    public double? WaistCm { get; set; }

    public double? HipCm { get; set; }

    public double? BodyFatPercentage { get; set; }

    public string? Notes { get; set; }
}