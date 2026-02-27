using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Howsee.Api.Common;
using Howsee.Application.Common;
using Howsee.Application.DTOs.requests.PropertyCategory;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.PropertyCategory;
using Howsee.Application.Interfaces.PropertyCategory;
using Howsee.Domain.Enums;

namespace Howsee.Api.Controllers;

[ApiController]
[Route("api/property-categories")]
public class PropertyCategoriesController(IPropertyCategoryService propertyCategoryService) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PropertyCategoryDto>>>> GetAll(CancellationToken cancellationToken = default)
    {
        var list = await propertyCategoryService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<List<PropertyCategoryDto>>.SuccessResponse(list.ToList()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PropertyCategoryDto>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var category = await propertyCategoryService.GetByIdAsync(id, cancellationToken);
        return category == null
            ? NotFound(ApiResponse<PropertyCategoryDto>.ErrorResponse("Property category not found.", code: ErrorCodes.PropertyCategoryNotFound))
            : Ok(ApiResponse<PropertyCategoryDto>.SuccessResponse(category));
    }

    [Authorize(Policy = nameof(UserRole.Administrator))]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PropertyCategoryDto>>> Create([FromBody] CreatePropertyCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await propertyCategoryService.CreateAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Policy = nameof(UserRole.Administrator))]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<PropertyCategoryDto>>> Update(int id, [FromBody] UpdatePropertyCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await propertyCategoryService.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result) : result.Code == ErrorCodes.PropertyCategoryNotFound ? NotFound(result) : BadRequest(result);
    }

    [Authorize(Policy = nameof(UserRole.Administrator))]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id, CancellationToken cancellationToken = default)
    {
        var result = await propertyCategoryService.DeleteAsync(id, cancellationToken);
        return result.Success ? Ok(result) : result.Code == ErrorCodes.PropertyCategoryNotFound ? NotFound(result) : BadRequest(result);
    }
}
