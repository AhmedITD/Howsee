namespace Howsee.Application.DTOs.responses.Pricing;

public class SubscriptionDto
{
    public int Id { get; set; }
    public string PlanKey { get; set; } = null!;
    public string? PlanName { get; set; }
    public DateTime EndDate { get; set; }
}
