using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Howsee.Api.Common;
using Howsee.Application.Common;
using Howsee.Application.DTOs.requests.Tour;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Tour;
using Howsee.Application.Interfaces.Auth;
using Howsee.Application.Interfaces.Tours;

namespace Howsee.Api.Controllers;

[ApiController]
[Route("api/tours")]
public class TourController(ITourService tourService, ICurrentUser currentUser, IMatterportApiClient matterportApiClient) : BaseController
{
    [Authorize]
    [HttpGet("matterport-models")]
    public async Task<ActionResult<ApiResponse<MatterportModelListResult>>> ListMatterportModels(
        [FromQuery] string? query,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? offset = null,
        CancellationToken cancellationToken = default)
    {
        if (!matterportApiClient.IsConfigured)
            return Ok(ApiResponse<MatterportModelListResult>.SuccessResponse(new MatterportModelListResult { Results = [], NextOffset = null }));
        try
        {
            var result = await matterportApiClient.ListModelsAsync(query ?? "*", Math.Clamp(pageSize, 1, 200), offset, cancellationToken);
            return Ok(ApiResponse<MatterportModelListResult>.SuccessResponse(result));
        }
        catch (MatterportApiException ex)
        {
            return BadRequest(ApiResponse<MatterportModelListResult>.ErrorResponse(ex.Message ?? "Matterport API error."));
        }
    }

    [Authorize]
    [HttpGet("matterport-models/{modelId}")]
    public async Task<ActionResult<ApiResponse<MatterportModelDetails?>>> GetMatterportModel(string modelId, CancellationToken cancellationToken = default)
    {
        if (!matterportApiClient.IsConfigured || string.IsNullOrWhiteSpace(modelId))
            return Ok(ApiResponse<MatterportModelDetails?>.SuccessResponse(null));
        try
        {
            var details = await matterportApiClient.GetModelDetailsAsync(modelId, cancellationToken);
            return Ok(ApiResponse<MatterportModelDetails?>.SuccessResponse(details));
        }
        catch (MatterportApiException ex)
        {
            var isLocked = ex.ErrorCode == "model.locked" || (ex.Message?.Contains("model.locked", StringComparison.OrdinalIgnoreCase) ?? false);
            var code = isLocked ? ErrorCodes.MatterportModelLocked : null;
            var message = isLocked ? "This Matterport model is locked. Unlock it in your Matterport account to access details." : (ex.Message ?? "Matterport API error.");
            return BadRequest(ApiResponse<MatterportModelDetails?>.ErrorResponse(message, code: code));
        }
    }

    [Authorize]
    [HttpGet("matterport-models/{modelId}/locations")]
    public async Task<ActionResult<ApiResponse<List<MatterportLocationInfo>>>> GetMatterportModelLocations(string modelId, CancellationToken cancellationToken = default)
    {
        if (!matterportApiClient.IsConfigured || string.IsNullOrWhiteSpace(modelId))
            return Ok(ApiResponse<List<MatterportLocationInfo>>.SuccessResponse([]));
        var list = await matterportApiClient.GetModelLocationsAsync(modelId, cancellationToken);
        return Ok(ApiResponse<List<MatterportLocationInfo>>.SuccessResponse(list.ToList()));
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TourResponse>>>> ListTours(CancellationToken cancellationToken = default)
    {
        var result = await tourService.ListTours(currentUser.Id, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TourResponse>>> GetTour(int id, CancellationToken cancellationToken = default)
    {
        var result = await tourService.GetTour(id, currentUser.Id, cancellationToken);
        return result.Data != null ? Ok(result) : NotFound(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TourResponse>>> CreateTour([FromBody] CreateTourRequest request, CancellationToken cancellationToken = default)
    {
        var result = await tourService.CreateTour(request, currentUser.Id, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<TourResponse>>> UpdateTour(int id, [FromBody] UpdateTourRequest request, CancellationToken cancellationToken = default)
    {
        var result = await tourService.UpdateTour(id, request, currentUser.Id, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTour(int id, CancellationToken cancellationToken = default)
    {
        var result = await tourService.DeleteTour(id, currentUser.Id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
