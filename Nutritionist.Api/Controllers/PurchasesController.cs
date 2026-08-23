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
[Authorize(Roles = nameof(UserRole.Client))]
public class PurchasesController : ControllerBase
{
    private readonly NutritionDbContext _db;

    public PurchasesController(NutritionDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePurchase(
        CreatePurchaseRequest request)
    {
        var userId = GetUserId();

        var client = await _db.ClientProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (client is null)
            return NotFound(new { message = "Client profile not found." });

        var package = await _db.Packages
            .FirstOrDefaultAsync(x =>
                x.Id == request.PackageId &&
                x.IsActive);

        if (package is null)
            return NotFound(new { message = "Package not found." });

        var hasActivePurchase = await _db.Purchases.AnyAsync(x =>
            x.ClientProfileId == client.Id &&
            x.Status == PurchaseStatus.Paid &&
            x.EndDateUtc >= DateTime.UtcNow);

        if (hasActivePurchase)
        {
            return Conflict(new
            {
                message = "You already have an active package."
            });
        }

        var startDate = DateTime.UtcNow;

        var purchase = new Purchase
        {
            ClientProfileId = client.Id,
            PackageId = package.Id,
            Amount = package.Price,
            Status = PurchaseStatus.Pending,
            StartDateUtc = startDate,
            EndDateUtc = startDate.AddDays(package.DurationWeeks * 7)
        };

        _db.Purchases.Add(purchase);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            purchaseId = purchase.Id,
            package = package.Name,
            amount = purchase.Amount,
            status = purchase.Status.ToString(),
            startDate = purchase.StartDateUtc,
            endDate = purchase.EndDateUtc,
            message = "Purchase created. Payment is pending."
        });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyPurchases()
    {
        var userId = GetUserId();

        var client = await _db.ClientProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (client is null)
            return NotFound(new { message = "Client profile not found." });

        var purchases = await _db.Purchases
            .AsNoTracking()
            .Where(x => x.ClientProfileId == client.Id)
            .Include(x => x.Package)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new
            {
                x.Id,
                Package = x.Package.Name,
                x.Amount,
                Status = x.Status.ToString(),
                x.StartDateUtc,
                x.EndDateUtc,
                x.PaymentReference,
                x.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(purchases);
    }

    // Development-only endpoint.
    // This simulates a successful payment until Razorpay/Stripe is integrated.
    [HttpPost("{purchaseId:guid}/simulate-payment")]
    public async Task<IActionResult> SimulatePayment(Guid purchaseId)
    {
        var userId = GetUserId();

        var client = await _db.ClientProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (client is null)
            return NotFound(new { message = "Client profile not found." });

        var purchase = await _db.Purchases
            .FirstOrDefaultAsync(x =>
                x.Id == purchaseId &&
                x.ClientProfileId == client.Id);

        if (purchase is null)
            return NotFound(new { message = "Purchase not found." });

        if (purchase.Status == PurchaseStatus.Paid)
            return BadRequest(new { message = "Purchase is already paid." });

        purchase.Status = PurchaseStatus.Paid;
        purchase.PaidAtUtc = DateTime.UtcNow;
        purchase.PaymentReference =
            $"DEMO-{Guid.NewGuid():N}";

        await _db.SaveChangesAsync();

        return Ok(new
        {
            purchaseId = purchase.Id,
            status = purchase.Status.ToString(),
            paymentReference = purchase.PaymentReference,
            message = "Demo payment completed successfully."
        });
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException();

        return userId;
    }
}