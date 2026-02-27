using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Listings;
using Howsee.Domain.Enums;

namespace Howsee.Application.Interfaces.Listings;

public interface IListingService
{
    /// <summary>List active listings for buyers with optional filters.</summary>
    Task<IReadOnlyList<ListingSummaryDto>> ListForBuyerAsync(
        ListingType? listingType = null,
        Howsee.Domain.Enums.PropertyCategory? category = null,
        int? currencyId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default);

    /// <summary>Get a single listing by id for buyer (includes full property).</summary>
    Task<ApiResponse<ListingDetailDto>> GetForBuyerAsync(int listingId, CancellationToken cancellationToken = default);
}
