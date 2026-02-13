using Howsee.Domain.Enums;
using Howsee.Domain.Interfaces;

namespace Howsee.Domain.Entities;

public class Invoice : ISoftDeletable, IAuditable
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "IQD";
    public string? Description { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string? QiPaymentId { get; set; }
    public DateTime? PaidAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    public int? PricingPlanId { get; set; }

    public User User { get; set; } = null!;
    public PricingPlan? PricingPlan { get; set; }
    public Subscription? Subscription { get; set; }
}
