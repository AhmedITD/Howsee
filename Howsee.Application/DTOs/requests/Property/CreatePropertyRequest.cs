using Howsee.Domain.Enums;

namespace Howsee.Application.DTOs.requests.Property;

public class CreatePropertyRequest
{
    public PropertyCategory Category { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public string? Description { get; set; }
    public decimal? Area { get; set; }
    public bool Active { get; set; } = true;
    public int? TourId { get; set; }
    public string? Address { get; set; }
    public string? Locality { get; set; }
    public string? AdministrativeArea { get; set; }
    public string? CountryCode { get; set; }
    public string? PostalCode { get; set; }
}
