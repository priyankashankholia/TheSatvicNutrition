using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nutritionist.Api.Data;
using Nutritionist.Api.DTOs;

namespace Nutritionist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    private readonly NutritionDbContext _db;

    public PackagesController(NutritionDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PackageResponse>>> GetPackages()
    {
        var packages = await _db.Packages
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Price)
            .Select(x => new PackageResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                DurationWeeks = x.DurationWeeks,
                AppointmentsPerWeek = x.AppointmentsPerWeek,
                IncludesDietPlan = x.IncludesDietPlan,
                IncludesProgressTracking = x.IncludesProgressTracking,
                IncludesMessaging = x.IncludesMessaging,
                IncludesPhotoTracking = x.IncludesPhotoTracking
            })
            .ToListAsync();

        return Ok(packages);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PackageResponse>> GetPackage(Guid id)
    {
        var package = await _db.Packages
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new PackageResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                DurationWeeks = x.DurationWeeks,
                AppointmentsPerWeek = x.AppointmentsPerWeek,
                IncludesDietPlan = x.IncludesDietPlan,
                IncludesProgressTracking = x.IncludesProgressTracking,
                IncludesMessaging = x.IncludesMessaging,
                IncludesPhotoTracking = x.IncludesPhotoTracking
            })
            .FirstOrDefaultAsync();

        if (package is null)
            return NotFound(new { message = "Package not found." });

        return Ok(package);
    }
}