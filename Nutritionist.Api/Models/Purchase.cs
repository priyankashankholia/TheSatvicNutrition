namespace Nutritionist.Api.Models;

public enum PurchaseStatus
{
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Cancelled = 4,
    Refunded = 5
}

public class Purchase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClientProfileId { get; set; }

    public ClientProfile ClientProfile { get; set; } = null!;

    public Guid PackageId { get; set; }

    public Package Package { get; set; } = null!;

    public decimal Amount { get; set; }

    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;

    public string? PaymentReference { get; set; }

    public DateTime? PaidAtUtc { get; set; }

    public DateTime StartDateUtc { get; set; }

    public DateTime EndDateUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}