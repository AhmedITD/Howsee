using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Howsee.Api.Common;
using Howsee.Application.Common;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Tour;
using Howsee.Application.Interfaces.Tours;

namespace Howsee.Api.Controllers;

[ApiController]
[Route("api/tour")]
[EnableRateLimiting("tour-public")]
public class TourViewController(ITourService tourService) : BaseController
{
    [HttpGet("view/{token}")]
    public async Task<ActionResult<ApiResponse<TourViewConfigResponse>>> GetViewConfig(
        string token,
        [FromQuery] string? password,
        CancellationToken cancellationToken = default)
    {
        var result = await tourService.GetViewConfigAsync(token, password, cancellationToken);
        if (!result.Success)
            return result.Code is ErrorCodes.TourNotFound or ErrorCodes.InvalidTourToken ? NotFound(result) : StatusCode(403, result);
        return Ok(result);
    }
}
