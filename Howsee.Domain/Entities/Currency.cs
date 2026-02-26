namespace Howsee.Domain.Entities;

public class Currency
{
    public int Id { get; set; }
    /// <summary>ISO 4217 e.g. USD, EUR, IQD.</summary>
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Symbol { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<PricingPlan> PricingPlans { get; set; } = new List<PricingPlan>();
    public ICollection<PropertyListing> PropertyListings { get; set; } = new List<PropertyListing>();
    public ICollection<RentOffer> RentOffers { get; set; } = new List<RentOffer>();
}
