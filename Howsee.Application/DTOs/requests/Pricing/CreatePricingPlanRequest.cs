using Howsee.Domain.Enums;

namespace Howsee.Application.DTOs.requests.Pricing;

public class CreatePricingPlanRequest
{
    public required string Key { get; set; }
    public string? Name { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IQD";
    public required string Unit { get; set; }
    public UserRole? Role { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
