namespace Howsee.Application.DTOs.requests.Tour;

public class UpdateTourRequest
{
    public string? Title { get; set; }
    public string? MatterportModelId { get; set; }
    public string? StartSweepId { get; set; }
    public string? Password { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool? IsActive { get; set; }
}
