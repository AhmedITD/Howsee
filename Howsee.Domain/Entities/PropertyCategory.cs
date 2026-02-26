namespace Howsee.Domain.Entities;

/// <summary>Lookup table (docs: property_categories). Values: House, Apartment, Office, Villa, Other.</summary>
public class PropertyCategoryLookup
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
