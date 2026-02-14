using Howsee.Domain.Enums;

namespace Howsee.Domain.Entities;

public class PricingPlan
{
    public int Id { get; set; }
    public required string Key { get; set; }
    public string? Name { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IQD";
    /// <summary>Billing unit: "month" or "tour".</summary>
    public required string Unit { get; set; }
    /// <summary>When set, paying for this plan assigns this role to the user.</summary>
    public UserRole? Role { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
