using Howsee.Application.DTOs.requests.Currency;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Currency;

namespace Howsee.Application.Interfaces.Currency;

public interface ICurrencyService
{
    Task<IReadOnlyList<CurrencyDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<CurrencyDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CurrencyDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<ApiResponse<CurrencyDto>> CreateAsync(CreateCurrencyRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CurrencyDto>> UpdateAsync(int id, UpdateCurrencyRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
