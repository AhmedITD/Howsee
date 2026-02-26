using Howsee.Domain.Entities.Common;
using Howsee.Domain.Enums;
using Howsee.Domain.Interfaces;

namespace Howsee.Domain.Entities;

public class Property : BaseEntity, IAuditable
{
    public int OwnerId { get; set; }
    public PropertyCategory Category { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public string? Description { get; set; }
    public decimal? Area { get; set; }
    public decimal? Price { get; set; }
    public bool Active { get; set; } = true;
    public int? TourId { get; set; }
    public PropertyAddress Address { get; set; } = new();

    public int? UpdatedBy { get; set; }

    public User Owner { get; set; } = null!;
    public Tour? Tour { get; set; }
    public ICollection<PropertyListing> Listings { get; set; } = new List<PropertyListing>();
    public ICollection<PropertyImage> PropertyImages { get; set; } = new List<PropertyImage>();
    public ICollection<Save> Saves { get; set; } = new List<Save>();
}
