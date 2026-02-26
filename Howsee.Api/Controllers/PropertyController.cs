using Howsee.Api.Common;
using Howsee.Application.DTOs.requests.Property;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Property;
using Howsee.Application.Interfaces.Auth;
using Howsee.Application.Interfaces.Properties;
using Howsee.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Howsee.Api.Controllers;

[ApiController]
[Route("api/properties")]
public class PropertyController(IPropertyService propertyService, ICurrentUser currentUser) : BaseController
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PropertyResponse>>>> List(
        [FromQuery] bool? active,
        [FromQuery] PropertyCategory? category,
        [FromQuery] int? tourId,
        CancellationToken cancellationToken = default)
    {
        var result = await propertyService.List(currentUser.Id, active, category, tourId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PropertyResponse>>> Get(int id, CancellationToken cancellationToken = default)
    {
        var result = await propertyService.Get(id, currentUser.Id, cancellationToken);
        return result.Data != null ? Ok(result) : NotFound(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PropertyResponse>>> Create([FromBody] CreatePropertyRequest request, CancellationToken cancellationToken = default)
    {
        var result = await propertyService.Create(request, currentUser.Id, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<PropertyResponse>>> Update(int id, [FromBody] UpdatePropertyRequest request, CancellationToken cancellationToken = default)
    {
        var result = await propertyService.Update(id, request, currentUser.Id, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id, CancellationToken cancellationToken = default)
    {
        var result = await propertyService.Delete(id, currentUser.Id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
