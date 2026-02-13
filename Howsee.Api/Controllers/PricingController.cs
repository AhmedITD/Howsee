using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Howsee.Api.Common;
using Howsee.Application.DTOs.requests.Pricing;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Pricing;
using Howsee.Application.Common;
using Howsee.Application.Interfaces.Auth;
using Howsee.Application.Interfaces.Pricing;
using Howsee.Application.Interfaces.Subscriptions;
using Howsee.Domain.Enums;

namespace Howsee.Api.Controllers;

[ApiController]
[Route("api/pricing")]
public class PricingController(IPricingPlanService pricingPlanService) : BaseController
{
    /// <summary>List plans. Public: active only. Admin: use ?includeInactive=true for all.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PricingPlanDto>>>> GetPlans(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PricingPlanDto> plans;
        if (includeInactive && User.IsInRole(UserRole.Administrator.ToStringValue()))
            plans = await pricingPlanService.GetAllPlansAsync(includeInactive: true, cancellationToken);
        else
            plans = await pricingPlanService.GetActivePlansAsync(cancellationToken);
        return Ok(ApiResponse<List<PricingPlanDto>>.SuccessResponse(plans.ToList()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PricingPlanDto>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var plan = await pricingPlanService.GetByIdAsync(id, cancellationToken);
        return plan == null ? NotFound(ApiResponse<PricingPlanDto>.ErrorResponse("Pricing plan not found.", code: ErrorCodes.PricingPlanNotFound)) : Ok(ApiResponse<PricingPlanDto>.SuccessResponse(plan));
    }

    [Authorize(Policy = nameof(UserRole.Administrator))]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PricingPlanDto>>> Create([FromBody] CreatePricingPlanRequest request, CancellationToken cancellationToken = default)
    {
        var result = await pricingPlanService.CreateAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Policy = nameof(UserRole.Administrator))]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<PricingPlanDto>>> Update(int id, [FromBody] UpdatePricingPlanRequest request, CancellationToken cancellationToken = default)
    {
        var result = await pricingPlanService.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result) : result.Code == ErrorCodes.PricingPlanNotFound ? NotFound(result) : BadRequest(result);
    }

    [Authorize(Policy = nameof(UserRole.Administrator))]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id, CancellationToken cancellationToken = default)
    {
        var result = await pricingPlanService.DeleteAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

[ApiController]
[Route("api/subscription")]
public class SubscriptionController(
    ISubscriptionService subscriptionService,
    ICurrentUser currentUser) : BaseController
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<SubscriptionDto?>>> GetCurrent(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.Id;
        if (userId == 0) return Unauthorized();
        var subscription = await subscriptionService.GetCurrentSubscriptionAsync(userId, cancellationToken);
        return Ok(ApiResponse<SubscriptionDto?>.SuccessResponse(subscription));
    }
}
