using Howsee.Domain.Enums;

namespace Howsee.Application.DTOs.responses.Property;

/// <summary>Listing line for property detail (price/currency/type per listing).</summary>
public class ListingLineDto
{
    public int ListingId { get; set; }
    public ListingType ListingType { get; set; }
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string CurrencySymbol { get; set; } = null!;
}
