using Howsee.Domain.Enums;
using Howsee.Domain.Interfaces;

namespace Howsee.Domain.Entities;

public class Invoice : ISoftDeletable, IAuditable
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public int CurrencyId { get; set; }
    public string? Description { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    /// <summary>Wayl payment link id (wayl_payment_id in schema).</summary>
    public string? WaylPaymentId { get; set; }
    public DateTime? PaidAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    public int? PricingPlanId { get; set; }

    public User User { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
    public PricingPlan? PricingPlan { get; set; }
    public Subscription? Subscription { get; set; }
}
