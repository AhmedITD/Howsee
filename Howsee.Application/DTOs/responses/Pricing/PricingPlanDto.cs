namespace Howsee.Application.DTOs.responses.Pricing;

public class PricingPlanDto
{
    public int Id { get; set; }
    public string Key { get; set; } = null!;
    public string? Name { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IQD";
    public string Unit { get; set; } = null!;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
