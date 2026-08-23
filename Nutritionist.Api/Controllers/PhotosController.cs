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
public class PhotosController : ControllerBase
{
    private readonly NutritionDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public PhotosController(
        NutritionDbContext db,
        IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [HttpGet("mine")]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<IActionResult> GetMyPhotos()
    {
        var userId = GetUserId();

        var clientId = await _db.ClientProfiles
            .Where(x => x.UserId == userId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync();

        if (clientId is null)
            return NotFound(new { message = "Client profile not found." });

        var photos = await _db.ProgressPhotos
            .AsNoTracking()
            .Where(x => x.ClientProfileId == clientId)
            .OrderByDescending(x => x.UploadedAtUtc)
            .Select(x => new ProgressPhotoResponse
            {
                Id = x.Id,
                FileUrl = x.FileUrl,
                Type = x.Type.ToString(),
                Notes = x.Notes,
                UploadedAtUtc = x.UploadedAtUtc
            })
            .ToListAsync();

        return Ok(photos);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Client))]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] PhotoType type,
        [FromForm] string? notes)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Please select a photo." });

        if (!file.ContentType.StartsWith("image/"))
            return BadRequest(new { message = "Only image files are allowed." });

        var userId = GetUserId();

        var client = await _db.ClientProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (client is null)
            return NotFound(new { message = "Client profile not found." });

        var extension = Path.GetExtension(file.FileName);

        var allowedExtensions = new[]
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        if (!allowedExtensions.Contains(
                extension,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Supported formats: JPG, PNG and WEBP."
            });
        }

        var uploadsFolder = Path.Combine(
            _environment.ContentRootPath,
            "uploads",
            "progress");

        Directory.CreateDirectory(uploadsFolder);

        var fileName =
            $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        var physicalPath = Path.Combine(
            uploadsFolder,
            fileName);

        await using var stream =
            new FileStream(
                physicalPath,
                FileMode.CreateNew);

        await file.CopyToAsync(stream);

        var photo = new ProgressPhoto
        {
            ClientProfileId = client.Id,
            FileUrl = $"/uploads/progress/{fileName}",
            Type = type,
            Notes = notes
        };

        _db.ProgressPhotos.Add(photo);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            id = photo.Id,
            url = photo.FileUrl,
            message = "Progress photo uploaded successfully."
        });
    }

    [HttpGet("client/{clientId:guid}")]
    [Authorize(Roles = nameof(UserRole.Nutritionist))]
    public async Task<IActionResult> GetClientPhotos(Guid clientId)
    {
        var exists = await _db.ClientProfiles
            .AnyAsync(x => x.Id == clientId);

        if (!exists)
            return NotFound(new { message = "Client not found." });

        var photos = await _db.ProgressPhotos
            .AsNoTracking()
            .Where(x => x.ClientProfileId == clientId)
            .OrderByDescending(x => x.UploadedAtUtc)
            .Select(x => new ProgressPhotoResponse
            {
                Id = x.Id,
                FileUrl = x.FileUrl,
                Type = x.Type.ToString(),
                Notes = x.Notes,
                UploadedAtUtc = x.UploadedAtUtc
            })
            .ToListAsync();

        return Ok(photos);
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