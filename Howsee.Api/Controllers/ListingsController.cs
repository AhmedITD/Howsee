using Microsoft.AspNetCore.Mvc;
using Howsee.Api.Common;
using Howsee.Application.Common;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Listings;
using Howsee.Application.Interfaces.Listings;
using Howsee.Domain.Enums;

namespace Howsee.Api.Controllers;

/// <summary>Endpoints for buyers (end users) to browse property listings.</summary>
[ApiController]
[Route("api/listings")]
public class ListingsController(IListingService listingService) : BaseController
{
    /// <summary>List active property listings. Optional filters: listingType (Sale/Rent), category, currencyId, minPrice, maxPrice.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ListingSummaryDto>>>> List(
        [FromQuery] ListingType? listingType = null,
        [FromQuery] Howsee.Domain.Enums.PropertyCategory? category = null,
        [FromQuery] int? currencyId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        var list = await listingService.ListForBuyerAsync(listingType, category, currencyId, minPrice, maxPrice, cancellationToken);
        return Ok(ApiResponse<List<ListingSummaryDto>>.SuccessResponse(list.ToList()));
    }

    /// <summary>Get a single listing by id with full property details.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ListingDetailDto>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await listingService.GetForBuyerAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
