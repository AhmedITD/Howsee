using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Howsee.Api.Common;
using Howsee.Application.DTOs.requests.Tour;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Tour;
using Howsee.Application.Interfaces.Auth;
using Howsee.Application.Interfaces.Tours;

namespace Howsee.Api.Controllers;

[ApiController]
[Route("api/tours")]
public class TourController(ITourService tourService, ICurrentUser currentUser) : BaseController
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TourResponse>>>> ListTours(CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        if (userId == 0) return Unauthorized();
        var result = await tourService.ListTours(userId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TourResponse>>> GetTour(int id, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        if (userId == 0) return Unauthorized();
        var result = await tourService.GetTour(id, userId, cancellationToken);
        return result.Data != null ? Ok(result) : NotFound(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TourResponse>>> CreateTour([FromBody] CreateTourRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        if (userId == 0) return Unauthorized();
        var result = await tourService.CreateTour(request, userId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<TourResponse>>> UpdateTour(int id, [FromBody] UpdateTourRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        if (userId == 0) return Unauthorized();
        var result = await tourService.UpdateTour(id, request, userId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTour(int id, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        if (userId == 0) return Unauthorized();
        var result = await tourService.DeleteTour(id, userId, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
