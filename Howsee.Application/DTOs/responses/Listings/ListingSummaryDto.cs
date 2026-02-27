using Howsee.Domain.Enums;

namespace Howsee.Application.DTOs.responses.Listings;

/// <summary>Summary of a listing for buyer browse list.</summary>
public class ListingSummaryDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public ListingType ListingType { get; set; }
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string CurrencySymbol { get; set; } = null!;
    public Howsee.Domain.Enums.PropertyCategory Category { get; set; }
    public string? Address { get; set; }
    public string? Locality { get; set; }
    public string? AdministrativeArea { get; set; }
    public string? CountryCode { get; set; }
    public decimal? Area { get; set; }
    public int? TourId { get; set; }
    public string? TourShareToken { get; set; }
    public DateTime? CreatedAt { get; set; }
}
