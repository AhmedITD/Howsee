namespace Howsee.Domain.Entities;

public class Image
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public string? AltText { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Type { get; set; }
}
