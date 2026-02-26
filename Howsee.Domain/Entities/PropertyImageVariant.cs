namespace Howsee.Domain.Entities;

public class PropertyImageVariant
{
    public int Id { get; set; }
    public int PropertyImageId { get; set; }
    /// <summary>e.g. small, medium, large.</summary>
    public required string Resolution { get; set; }
    public required string Url { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long? FileSizeBytes { get; set; }

    public PropertyImage PropertyImage { get; set; } = null!;
}
