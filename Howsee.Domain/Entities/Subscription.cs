using Howsee.Domain.Enums;

namespace Howsee.Domain.Entities;

public class Subscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PricingPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public Guid? InvoiceId { get; set; }

    public User User { get; set; } = null!;
    public PricingPlan PricingPlan { get; set; } = null!;
    public Invoice? Invoice { get; set; }
}
