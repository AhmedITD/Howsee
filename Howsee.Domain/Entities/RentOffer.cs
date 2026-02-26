using Howsee.Domain.Enums;

namespace Howsee.Domain.Entities;

public class RentOffer
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public int TenantId { get; set; }
    public decimal OfferedPrice { get; set; }
    public int CurrencyId { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime ProposedStartDate { get; set; }
    public DateTime ProposedEndDate { get; set; }
    public RentOfferStatus Status { get; set; }
    public string? Message { get; set; }
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public PropertyListing Listing { get; set; } = null!;
    public User Tenant { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
}
