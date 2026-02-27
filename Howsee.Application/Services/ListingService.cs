using Howsee.Application.Common;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Listings;
using Howsee.Application.DTOs.responses.Property;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Listings;
using Howsee.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Howsee.Application.Services;

public class ListingService(IHowseeDbContext dbContext) : IListingService
{
    public async Task<IReadOnlyList<ListingSummaryDto>> ListForBuyerAsync(
        ListingType? listingType = null,
        Howsee.Domain.Enums.PropertyCategory? category = null,
        int? currencyId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.PropertyListings
            .AsNoTracking()
            .Include(l => l.Property).ThenInclude(p => p!.Tour)
            .Include(l => l.Currency)
            .Where(l => l.IsActive && l.Property != null && l.Property.Active);

        if (listingType.HasValue)
            query = query.Where(l => l.ListingType == listingType.Value);
        if (category.HasValue)
            query = query.Where(l => l.Property!.Category == category.Value);
        if (currencyId.HasValue)
            query = query.Where(l => l.CurrencyId == currencyId.Value);
        if (minPrice.HasValue)
            query = query.Where(l => l.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(l => l.Price <= maxPrice.Value);

        var list = await query
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new ListingSummaryDto
            {
                Id = l.Id,
                PropertyId = l.PropertyId,
                ListingType = l.ListingType,
                Price = l.Price,
                CurrencyCode = l.Currency.Code,
                CurrencySymbol = l.Currency.Symbol,
                Category = l.Property!.Category,
                Address = l.Property.Address != null ? l.Property.Address.Address : null,
                Locality = l.Property.Address != null ? l.Property.Address.Locality : null,
                AdministrativeArea = l.Property.Address != null ? l.Property.Address.AdministrativeArea : null,
                CountryCode = l.Property.Address != null ? l.Property.Address.CountryCode : null,
                Area = l.Property.Area,
                TourId = l.Property.TourId,
                TourShareToken = l.Property.Tour != null ? l.Property.Tour.ShareToken : null,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return list;
    }

    public async Task<ApiResponse<ListingDetailDto>> GetForBuyerAsync(int listingId, CancellationToken cancellationToken = default)
    {
        var listing = await dbContext.PropertyListings
            .AsNoTracking()
            .Include(l => l.Property).ThenInclude(p => p!.Tour)
            .Include(l => l.Currency)
            .FirstOrDefaultAsync(l => l.Id == listingId && l.IsActive, cancellationToken);

        if (listing?.Property == null || !listing.Property.Active)
            return ApiResponse<ListingDetailDto>.ErrorResponse("Listing not found.", code: ErrorCodes.ListingNotFound);

        var prop = listing.Property;
        var dto = new ListingDetailDto
        {
            Id = listing.Id,
            PropertyId = listing.PropertyId,
            CurrencyId = listing.CurrencyId,
            CurrencyCode = listing.Currency.Code,
            CurrencySymbol = listing.Currency.Symbol,
            ListingType = listing.ListingType,
            Price = listing.Price,
            CreatedAt = listing.CreatedAt,
            Property = new PropertyResponse
            {
                Id = prop.Id,
                Category = prop.Category,
                Lat = prop.Lat,
                Lng = prop.Lng,
                Description = prop.Description,
                Area = prop.Area,
                Price = prop.Price,
                Active = prop.Active,
                TourId = prop.TourId,
                TourTitle = prop.Tour?.Title,
                Address = prop.Address?.Address,
                Locality = prop.Address?.Locality,
                AdministrativeArea = prop.Address?.AdministrativeArea,
                CountryCode = prop.Address?.CountryCode,
                PostalCode = prop.Address?.PostalCode,
                CreatedAt = prop.CreatedAt
            }
        };

        return ApiResponse<ListingDetailDto>.SuccessResponse(dto);
    }
}
