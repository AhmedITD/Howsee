namespace Howsee.Domain.Entities;

public class PropertyImage
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int? SortOrder { get; set; }
    public string? AltText { get; set; }
    public DateTime? CreatedAt { get; set; }

    public Property Property { get; set; } = null!;
    public ICollection<PropertyImageVariant> Variants { get; set; } = new List<PropertyImageVariant>();
}
