using Howsee.Application.DTOs.requests.Pricing;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Pricing;

namespace Howsee.Application.Interfaces.Pricing;

public interface IPricingPlanService
{
    Task<IReadOnlyList<PricingPlanDto>> GetActivePlansAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PricingPlanDto>> GetAllPlansAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<PricingPlanDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<PricingPlanDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PricingPlanDto>> CreateAsync(CreatePricingPlanRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PricingPlanDto>> UpdateAsync(int id, UpdatePricingPlanRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
