using Howsee.Application.DTOs.responses.Pricing;

namespace Howsee.Application.Interfaces.Pricing;

public interface IPricingPlanService
{
    Task<IReadOnlyList<PricingPlanDto>> GetActivePlansAsync(CancellationToken cancellationToken = default);
    Task<PricingPlanDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
}
