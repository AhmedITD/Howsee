using Howsee.Application.Common;
using Howsee.Application.DTOs.requests.Currency;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Currency;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Currency;
using Howsee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Howsee.Application.Services;

public class CurrencyService(IHowseeDbContext dbContext) : ICurrencyService
{
    public async Task<IReadOnlyList<CurrencyDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Currencies.AsNoTracking();
        if (!includeInactive)
            query = query.Where(c => c.IsActive);
        return await query
            .OrderBy(c => c.Code)
            .Select(c => new CurrencyDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Symbol = c.Symbol,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CurrencyDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Currencies
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CurrencyDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Symbol = c.Symbol,
                IsActive = c.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CurrencyDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        return await dbContext.Currencies
            .AsNoTracking()
            .Where(c => c.Code == code)
            .Select(c => new CurrencyDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Symbol = c.Symbol,
                IsActive = c.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ApiResponse<CurrencyDto>> CreateAsync(CreateCurrencyRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await dbContext.Currencies.AnyAsync(c => c.Code == code, cancellationToken))
            return ApiResponse<CurrencyDto>.ErrorResponse("A currency with this code already exists.", code: ErrorCodes.CurrencyCodeExists);

        var currency = new Currency
        {
            Code = code,
            Name = request.Name.Trim(),
            Symbol = request.Symbol.Trim(),
            IsActive = request.IsActive
        };
        dbContext.Currencies.Add(currency);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = await GetByIdAsync(currency.Id, cancellationToken);
        return ApiResponse<CurrencyDto>.SuccessResponse(dto!);
    }

    public async Task<ApiResponse<CurrencyDto>> UpdateAsync(int id, UpdateCurrencyRequest request, CancellationToken cancellationToken = default)
    {
        var currency = await dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (currency == null)
            return ApiResponse<CurrencyDto>.ErrorResponse("Currency not found.", code: ErrorCodes.CurrencyNotFound);

        if (request.Name != null) currency.Name = request.Name.Trim();
        if (request.Symbol != null) currency.Symbol = request.Symbol.Trim();
        if (request.IsActive.HasValue) currency.IsActive = request.IsActive.Value;

        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = await GetByIdAsync(currency.Id, cancellationToken);
        return ApiResponse<CurrencyDto>.SuccessResponse(dto!);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var currency = await dbContext.Currencies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (currency == null)
            return ApiResponse<bool>.ErrorResponse("Currency not found.", code: ErrorCodes.CurrencyNotFound);

        var inUse = await dbContext.Invoices.AnyAsync(i => i.CurrencyId == id, cancellationToken)
                    || await dbContext.PricingPlans.AnyAsync(p => p.CurrencyId == id, cancellationToken);
        if (inUse)
            return ApiResponse<bool>.ErrorResponse("Currency is in use and cannot be deleted.");

        dbContext.Currencies.Remove(currency);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true);
    }
}
