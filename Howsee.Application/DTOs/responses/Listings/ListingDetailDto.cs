using Howsee.Application.DTOs.responses.Property;
using Howsee.Application.DTOs.responses.Listings;

namespace Howsee.Application.DTOs.responses.Listings;

/// <summary>Listing detail for buyer with full property info.</summary>
public class ListingDetailDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string CurrencySymbol { get; set; } = null!;
    public Howsee.Domain.Enums.ListingType ListingType { get; set; }
    public decimal Price { get; set; }
    public DateTime? CreatedAt { get; set; }
    public PropertyResponse Property { get; set; } = null!;
}
