namespace Howsee.Application.DTOs.responses.Property;

/// <summary>Property detail for buyer (property + tour link + listings).</summary>
public class PropertyDetailForBuyerDto
{
    public PropertyResponse Property { get; set; } = null!;
    public string? TourShareToken { get; set; }
    public List<ListingLineDto> Listings { get; set; } = new();
}
