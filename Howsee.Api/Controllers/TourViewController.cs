using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Tour;
using Howsee.Application.Interfaces.Tours;

namespace Howsee.Api.Controllers;

[ApiController]
[Route("api/tour")]
[EnableRateLimiting("tour-public")]
public class TourViewController(ITourService tourService) : ControllerBase
{
    [HttpGet("view/{token}")]
    public async Task<ActionResult<ApiResponse<TourViewConfigResponse>>> GetViewConfig(
        string token,
        [FromQuery] string? password,
        CancellationToken cancellationToken)
    {
        var result = await tourService.GetViewConfigAsync(token, password, cancellationToken);
        if (!result.Success)
        {
            if (result.Code == Application.Common.ErrorCodes.TourNotFound ||
                result.Code == Application.Common.ErrorCodes.InvalidTourToken)
                return NotFound(result);
            return StatusCode(403, result);
        }
        return Ok(result);
    }
}
