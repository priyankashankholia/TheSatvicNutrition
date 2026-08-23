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
public class MessagesController : ControllerBase
{
    private readonly NutritionDbContext _db;

    public MessagesController(NutritionDbContext db)
    {
        _db = db;
    }

    [HttpGet("mine")]
    [Authorize(Roles = nameof(UserRole.Client))]
    public async Task<IActionResult> GetMyMessages()
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

        var messages = await GetConversation(clientId.Value);

        return Ok(messages);
    }

    [HttpGet("client/{clientId:guid}")]
    [Authorize(Roles = nameof(UserRole.Nutritionist))]
    public async Task<IActionResult> GetClientMessages(Guid clientId)
    {
        var exists = await _db.ClientProfiles
            .AnyAsync(x => x.Id == clientId);

        if (!exists)
        {
            return NotFound(new
            {
                message = "Client not found."
            });
        }

        var messages = await GetConversation(clientId);

        return Ok(messages);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Send(
        [FromBody] SendMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(new
            {
                message = "Message cannot be empty."
            });
        }

        if (request.Content.Length > 2000)
        {
            return BadRequest(new
            {
                message = "Message cannot exceed 2000 characters."
            });
        }

        var senderId = GetUserId();

        var sender = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == senderId);

        if (sender is null || !sender.IsActive)
        {
            return Unauthorized();
        }

        var client = await _db.ClientProfiles
            .FirstOrDefaultAsync(x =>
                x.Id == request.ClientProfileId);

        if (client is null)
        {
            return NotFound(new
            {
                message = "Client not found."
            });
        }

        // A client can only send messages to their own conversation.
        if (sender.Role == UserRole.Client &&
            client.UserId != senderId)
        {
            return Forbid();
        }

        var message = new Message
        {
            ClientProfileId = client.Id,
            SenderUserId = senderId,
            Content = request.Content.Trim()
        };

        _db.Messages.Add(message);

        // Notify the other participant.
        var recipientId =
            sender.Role == UserRole.Client
                ? await _db.NutritionistProfiles
                    .Select(x => (Guid?)x.UserId)
                    .FirstOrDefaultAsync()
                : client.UserId;

        if (recipientId is not null)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = recipientId.Value,
                Type = NotificationType.Message,
                Title = "New message",
                Message = $"{sender.FirstName} sent you a message."
            });
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            id = message.Id,
            message = "Message sent successfully."
        });
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetUserId();

        var message = await _db.Messages
            .Include(x => x.ClientProfile)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (message is null)
        {
            return NotFound(new
            {
                message = "Message not found."
            });
        }

        // The sender cannot mark their own message as received.
        if (message.SenderUserId == userId)
        {
            return BadRequest(new
            {
                message = "Sender cannot mark their own message as read."
            });
        }

        var currentUser = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (currentUser is null)
            return Unauthorized();

        if (currentUser.Role == UserRole.Client &&
            message.ClientProfile.UserId != userId)
        {
            return Forbid();
        }

        message.IsRead = true;
        message.ReadAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Message marked as read."
        });
    }

    private async Task<List<MessageResponse>> GetConversation(
        Guid clientId)
    {
        return await _db.Messages
            .AsNoTracking()
            .Where(x => x.ClientProfileId == clientId)
            .OrderBy(x => x.SentAtUtc)
            .Select(x => new MessageResponse
            {
                Id = x.Id,
                ClientProfileId = x.ClientProfileId,
                SenderUserId = x.SenderUserId,
                SenderName =
                    x.SenderUser.FirstName + " " +
                    x.SenderUser.LastName,
                SenderRole = x.SenderUser.Role.ToString(),
                Content = x.Content,
                IsRead = x.IsRead,
                SentAtUtc = x.SentAtUtc
            })
            .ToListAsync();
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

public class SendMessageRequest
{
    public Guid ClientProfileId { get; set; }

    public string Content { get; set; } = string.Empty;
}