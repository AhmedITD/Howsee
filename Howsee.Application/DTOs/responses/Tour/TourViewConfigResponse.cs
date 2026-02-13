namespace Howsee.Application.DTOs.responses.Tour;

public class TourViewConfigResponse
{
    public required string MatterportModelId { get; set; }
    public required string ApplicationKey { get; set; }
    public string? StartSweepId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
