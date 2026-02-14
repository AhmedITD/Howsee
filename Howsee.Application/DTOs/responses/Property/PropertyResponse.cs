using Howsee.Domain.Enums;

namespace Howsee.Application.DTOs.responses.Property;

public class PropertyResponse
{
    public int Id { get; set; }
    public PropertyCategory Category { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public string? Description { get; set; }
    public decimal? Area { get; set; }
    public decimal? Price { get; set; }
    public bool Active { get; set; }
    public int? TourId { get; set; }
    public string? TourTitle { get; set; }
    public string? Address { get; set; }
    public string? Locality { get; set; }
    public string? AdministrativeArea { get; set; }
    public string? CountryCode { get; set; }
    public string? PostalCode { get; set; }
    public DateTime CreatedAt { get; set; }
}
