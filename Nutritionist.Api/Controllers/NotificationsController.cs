using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nutritionist.Api.Data;
using Nutritionist.Api.DTOs;

namespace Nutritionist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NutritionDbContext _db;

    public NotificationsController(NutritionDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetUserId();

        var notifications = await _db.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new NotificationResponse
            {
                Id = x.Id,
                Type = x.Type.ToString(),
                Title = x.Title,
                Message = x.Message,
                IsRead = x.IsRead,
                CreatedAtUtc = x.CreatedAtUtc,
                ReadAtUtc = x.ReadAtUtc
            })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetUserId();

        var notification = await _db.Notifications
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UserId == userId);

        if (notification is null)
        {
            return NotFound(new
            {
                message = "Notification not found."
            });
        }

        notification.IsRead = true;
        notification.ReadAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Notification marked as read."
        });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();

        var notifications = await _db.Notifications
            .Where(x =>
                x.UserId == userId &&
                !x.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "All notifications marked as read."
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