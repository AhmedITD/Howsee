namespace Howsee.Application.DTOs.requests.Tour;

public class CreateTourRequest
{
    public required string Title { get; set; }
    public required string MatterportModelId { get; set; }
    public string? StartSweepId { get; set; }
    public string? Password { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
