using Howsee.Domain.Enums;

namespace Howsee.Application.DTOs.responses.Property;

/// <summary>Property summary for buyer browse list (properties with active listings).</summary>
public class PropertySummaryForBuyerDto
{
    public int Id { get; set; }
    public Howsee.Domain.Enums.PropertyCategory Category { get; set; }
    public string? Address { get; set; }
    public string? Locality { get; set; }
    public string? AdministrativeArea { get; set; }
    public string? CountryCode { get; set; }
    public decimal? Area { get; set; }
    public int? TourId { get; set; }
    public string? TourShareToken { get; set; }
    public DateTime CreatedAt { get; set; }
    /// <summary>Primary listing price (from first active listing).</summary>
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string CurrencySymbol { get; set; } = null!;
    public ListingType ListingType { get; set; }
}
