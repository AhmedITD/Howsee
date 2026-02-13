using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Howsee.Api.Common;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Pricing;
using Howsee.Application.Interfaces.Auth;
using Howsee.Application.Interfaces.Pricing;
using Howsee.Application.Interfaces.Subscriptions;

namespace Howsee.Api.Controllers;

[ApiController]
[Route("api/pricing")]
public class PricingController(IPricingPlanService pricingPlanService) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PricingPlanDto>>>> GetPlans(CancellationToken cancellationToken = default)
    {
        var plans = await pricingPlanService.GetActivePlansAsync(cancellationToken);
        return Ok(ApiResponse<List<PricingPlanDto>>.SuccessResponse(plans.ToList()));
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
