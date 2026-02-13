using Howsee.Application.DTOs.responses.Pricing;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Pricing;
using Microsoft.EntityFrameworkCore;

namespace Howsee.Application.Services;

public class PricingPlanService(IHowseeDbContext dbContext) : IPricingPlanService
{
    public async Task<IReadOnlyList<PricingPlanDto>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.PricingPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => new PricingPlanDto
            {
                Id = p.Id,
                Key = p.Key,
                Name = p.Name,
                Amount = p.Amount,
                Currency = p.Currency,
                Unit = p.Unit
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PricingPlanDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var plan = await dbContext.PricingPlans
            .AsNoTracking()
            .Where(p => p.Key == key && p.IsActive)
            .Select(p => new PricingPlanDto
            {
                Id = p.Id,
                Key = p.Key,
                Name = p.Name,
                Amount = p.Amount,
                Currency = p.Currency,
                Unit = p.Unit
            })
            .FirstOrDefaultAsync(cancellationToken);

        return plan;
    }
}
