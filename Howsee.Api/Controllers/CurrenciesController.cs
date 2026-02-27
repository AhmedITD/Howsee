using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Howsee.Api.Common;
using Howsee.Application.Common;
using Howsee.Application.DTOs.requests.Currency;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Currency;
using Howsee.Application.Interfaces.Currency;
using Howsee.Domain.Enums;

namespace Howsee.Api.Controllers;

[ApiController]
[Route("api/currencies")]
public class CurrenciesController(ICurrencyService currencyService) : BaseController
{
    /// <summary>List currencies. Public: active only. Admin: use ?includeInactive=true for all.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CurrencyDto>>>> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var list = includeInactive && User.IsInRole(UserRole.Administrator.ToStringValue())
            ? await currencyService.GetAllAsync(includeInactive: true, cancellationToken)
            : await currencyService.GetAllAsync(includeInactive: false, cancellationToken);
        return Ok(ApiResponse<List<CurrencyDto>>.SuccessResponse(list.ToList()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CurrencyDto>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var currency = await currencyService.GetByIdAsync(id, cancellationToken);
        return currency == null
            ? NotFound(ApiResponse<CurrencyDto>.ErrorResponse("Currency not found.", code: ErrorCodes.CurrencyNotFound))
            : Ok(ApiResponse<CurrencyDto>.SuccessResponse(currency));
    }

    [Authorize(Policy = nameof(UserRole.Administrator))]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CurrencyDto>>> Create([FromBody] CreateCurrencyRequest request, CancellationToken cancellationToken = default)
    {
        var result = await currencyService.CreateAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Policy = nameof(UserRole.Administrator))]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CurrencyDto>>> Update(int id, [FromBody] UpdateCurrencyRequest request, CancellationToken cancellationToken = default)
    {
        var result = await currencyService.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result) : result.Code == ErrorCodes.CurrencyNotFound ? NotFound(result) : BadRequest(result);
    }

    [Authorize(Policy = nameof(UserRole.Administrator))]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id, CancellationToken cancellationToken = default)
    {
        var result = await currencyService.DeleteAsync(id, cancellationToken);
        return result.Success ? Ok(result) : result.Code == ErrorCodes.CurrencyNotFound ? NotFound(result) : BadRequest(result);
    }
}
