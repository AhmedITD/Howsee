namespace Howsee.Application.DTOs.responses.Tour;

public class TourResponse
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string MatterportModelId { get; set; }
    public string? StartSweepId { get; set; }
    public bool HasPassword { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ShareToken { get; set; }
}
