using Howsee.Domain.Enums;

namespace Howsee.Application.DTOs.requests.Pricing;

public class UpdatePricingPlanRequest
{
    public string? Name { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public string? Unit { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}
