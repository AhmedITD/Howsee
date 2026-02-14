using Howsee.Application.Common;
using Howsee.Application.DTOs.requests.Pricing;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Pricing;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Pricing;
using Howsee.Domain.Entities;
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
                Unit = p.Unit,
                Role = p.Role,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PricingPlanDto>> GetAllPlansAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = dbContext.PricingPlans.AsNoTracking();
        if (!includeInactive)
            query = query.Where(p => p.IsActive);
        return await query
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .Select(p => new PricingPlanDto
            {
                Id = p.Id,
                Key = p.Key,
                Name = p.Name,
                Amount = p.Amount,
                Currency = p.Currency,
                Unit = p.Unit,
                Role = p.Role,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PricingPlanDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        return await dbContext.PricingPlans
            .AsNoTracking()
            .Where(p => p.Key == key && p.IsActive)
            .Select(p => new PricingPlanDto
            {
                Id = p.Id,
                Key = p.Key,
                Name = p.Name,
                Amount = p.Amount,
                Currency = p.Currency,
                Unit = p.Unit,
                Role = p.Role,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PricingPlanDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.PricingPlans
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PricingPlanDto
            {
                Id = p.Id,
                Key = p.Key,
                Name = p.Name,
                Amount = p.Amount,
                Currency = p.Currency,
                Unit = p.Unit,
                Role = p.Role,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ApiResponse<PricingPlanDto>> CreateAsync(CreatePricingPlanRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await dbContext.PricingPlans.AnyAsync(p => p.Key == request.Key, cancellationToken);
        if (exists)
            return ApiResponse<PricingPlanDto>.ErrorResponse("A plan with this key already exists.", code: ErrorCodes.PricingPlanKeyExists);

        var plan = new PricingPlan
        {
            Key = request.Key,
            Name = request.Name,
            Amount = request.Amount,
            Currency = request.Currency ?? "IQD",
            Unit = request.Unit,
            Role = request.Role,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder
        };
        dbContext.PricingPlans.Add(plan);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = await GetByIdAsync(plan.Id, cancellationToken);
        return ApiResponse<PricingPlanDto>.SuccessResponse(dto!);
    }

    public async Task<ApiResponse<PricingPlanDto>> UpdateAsync(int id, UpdatePricingPlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await dbContext.PricingPlans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (plan == null)
            return ApiResponse<PricingPlanDto>.ErrorResponse("Pricing plan not found.", code: ErrorCodes.PricingPlanNotFound);

        if (request.Name != null) plan.Name = request.Name;
        if (request.Amount.HasValue) plan.Amount = request.Amount.Value;
        if (request.Currency != null) plan.Currency = request.Currency;
        if (request.Unit != null) plan.Unit = request.Unit;
        if (request.Role.HasValue) plan.Role = request.Role;
        if (request.IsActive.HasValue) plan.IsActive = request.IsActive.Value;
        if (request.SortOrder.HasValue) plan.SortOrder = request.SortOrder.Value;

        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = await GetByIdAsync(plan.Id, cancellationToken);
        return ApiResponse<PricingPlanDto>.SuccessResponse(dto!);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var plan = await dbContext.PricingPlans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (plan == null)
            return ApiResponse<bool>.ErrorResponse("Pricing plan not found.", code: ErrorCodes.PricingPlanNotFound);

        plan.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true);
    }
}
