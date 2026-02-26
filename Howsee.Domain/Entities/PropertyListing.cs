using Howsee.Domain.Enums;

namespace Howsee.Domain.Entities;

public class PropertyListing
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public ListingType ListingType { get; set; }
    public decimal Price { get; set; }
    public int CurrencyId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Property Property { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
}
