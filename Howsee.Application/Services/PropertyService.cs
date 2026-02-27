using Howsee.Application.Common;
using Howsee.Application.DTOs.requests.Property;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Property;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Properties;
using Howsee.Domain.Entities;
using Howsee.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Howsee.Application.Services;

public class PropertyService(IHowseeDbContext dbContext) : IPropertyService
{
    public async Task<ApiResponse<PropertyResponse>> Create(CreatePropertyRequest request, int ownerId, CancellationToken cancellationToken = default)
    {
        if (request.TourId.HasValue)
        {
            var tourExists = await dbContext.Tours.AnyAsync(t => t.Id == request.TourId.Value && t.OwnerId == ownerId, cancellationToken);
            if (!tourExists)
                return ApiResponse<PropertyResponse>.ErrorResponse("Tour not found or access denied.", code: ErrorCodes.TourNotFound);
        }

        var property = new Property
        {
            OwnerId = ownerId,
            Category = request.Category,
            Lat = request.Lat,
            Lng = request.Lng,
            Description = request.Description,
            Area = request.Area,
            Price = request.Price,
            Active = request.Active,
            TourId = request.TourId,
            Address = new PropertyAddress
            {
                Address = request.Address,
                Locality = request.Locality,
                AdministrativeArea = request.AdministrativeArea,
                CountryCode = request.CountryCode,
                PostalCode = request.PostalCode
            }
        };
        dbContext.Properties.Add(property);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<PropertyResponse>.SuccessResponse(ToResponse(property));
    }

    public async Task<ApiResponse<PropertyResponse>> Update(int propertyId, UpdatePropertyRequest request, int ownerId, CancellationToken cancellationToken = default)
    {
        var property = await dbContext.Properties
            .Include(p => p.Tour)
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.OwnerId == ownerId, cancellationToken);
        if (property == null)
            return ApiResponse<PropertyResponse>.ErrorResponse("Property not found.", code: ErrorCodes.PropertyNotFound);

        if (request.TourId.HasValue)
        {
            var tourExists = await dbContext.Tours.AnyAsync(t => t.Id == request.TourId.Value && t.OwnerId == ownerId, cancellationToken);
            if (!tourExists)
                return ApiResponse<PropertyResponse>.ErrorResponse("Tour not found or access denied.", code: ErrorCodes.TourNotFound);
        }

        if (request.Category.HasValue) property.Category = request.Category.Value;
        if (request.Lat.HasValue) property.Lat = request.Lat;
        if (request.Lng.HasValue) property.Lng = request.Lng;
        if (request.Description != null) property.Description = request.Description;
        if (request.Area.HasValue) property.Area = request.Area;
        if (request.Price.HasValue) property.Price = request.Price;
        if (request.Active.HasValue) property.Active = request.Active.Value;
        if (request.ClearTourId == true) property.TourId = null;
        else if (request.TourId.HasValue) property.TourId = request.TourId;

        property.Address ??= new PropertyAddress();
        if (request.Address != null) property.Address.Address = request.Address;
        if (request.Locality != null) property.Address.Locality = request.Locality;
        if (request.AdministrativeArea != null) property.Address.AdministrativeArea = request.AdministrativeArea;
        if (request.CountryCode != null) property.Address.CountryCode = request.CountryCode;
        if (request.PostalCode != null) property.Address.PostalCode = request.PostalCode;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<PropertyResponse>.SuccessResponse(ToResponse(property));
    }

    public async Task<ApiResponse<PropertyResponse>> Get(int propertyId, int ownerId, CancellationToken cancellationToken = default)
    {
        var property = await dbContext.Properties
            .Include(p => p.Tour)
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.OwnerId == ownerId, cancellationToken);
        if (property == null)
            return ApiResponse<PropertyResponse>.ErrorResponse("Property not found.", code: ErrorCodes.PropertyNotFound);
        return ApiResponse<PropertyResponse>.SuccessResponse(ToResponse(property));
    }

    public async Task<ApiResponse<List<PropertyResponse>>> List(int ownerId, bool? active = null, Howsee.Domain.Enums.PropertyCategory? category = null, int? tourId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Properties
            .Include(p => p.Tour)
            .Where(p => p.OwnerId == ownerId);

        if (active.HasValue) query = query.Where(p => p.Active == active.Value);
        if (category.HasValue) query = query.Where(p => p.Category == category.Value);
        if (tourId.HasValue) query = query.Where(p => p.TourId == tourId.Value);

        var list = await query.OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);
        var response = list.Select(ToResponse).ToList();
        return ApiResponse<List<PropertyResponse>>.SuccessResponse(response);
    }

    public async Task<IReadOnlyList<PropertySummaryForBuyerDto>> ListForBuyerAsync(ListingType? listingType = null, Howsee.Domain.Enums.PropertyCategory? category = null, int? currencyId = null, decimal? minPrice = null, decimal? maxPrice = null, CancellationToken cancellationToken = default)
    {
        var listingQuery = dbContext.PropertyListings
            .AsNoTracking()
            .Include(l => l.Property).ThenInclude(p => p!.Tour)
            .Include(l => l.Currency)
            .Where(l => l.IsActive && l.Property != null && l.Property.Active);

        if (listingType.HasValue)
            listingQuery = listingQuery.Where(l => l.ListingType == listingType.Value);
        if (category.HasValue)
            listingQuery = listingQuery.Where(l => l.Property!.Category == category.Value);
        if (currencyId.HasValue)
            listingQuery = listingQuery.Where(l => l.CurrencyId == currencyId.Value);
        if (minPrice.HasValue)
            listingQuery = listingQuery.Where(l => l.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            listingQuery = listingQuery.Where(l => l.Price <= maxPrice.Value);

        var listingRows = await listingQuery
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new
            {
                l.PropertyId,
                l.Price,
                CurrencyCode = l.Currency.Code,
                CurrencySymbol = l.Currency.Symbol,
                l.ListingType
            })
            .ToListAsync(cancellationToken);

        var byProperty = listingRows
            .GroupBy(x => x.PropertyId)
            .ToDictionary(g => g.Key, g => g.First());
        var propertyIds = byProperty.Keys.ToList();
        if (propertyIds.Count == 0)
            return Array.Empty<PropertySummaryForBuyerDto>();

        var properties = await dbContext.Properties
            .AsNoTracking()
            .Include(p => p.Tour)
            .Where(p => propertyIds.Contains(p.Id))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return properties.Select(p =>
        {
            var primary = byProperty[p.Id];
            return new PropertySummaryForBuyerDto
            {
                Id = p.Id,
                Category = p.Category,
                Address = p.Address?.Address,
                Locality = p.Address?.Locality,
                AdministrativeArea = p.Address?.AdministrativeArea,
                CountryCode = p.Address?.CountryCode,
                Area = p.Area,
                TourId = p.TourId,
                TourShareToken = p.Tour?.ShareToken,
                CreatedAt = p.CreatedAt,
                Price = primary.Price,
                CurrencyCode = primary.CurrencyCode,
                CurrencySymbol = primary.CurrencySymbol,
                ListingType = primary.ListingType
            };
        }).ToList();
    }

    public async Task<ApiResponse<PropertyDetailForBuyerDto>> GetForBuyerAsync(int propertyId, CancellationToken cancellationToken = default)
    {
        var property = await dbContext.Properties
            .AsNoTracking()
            .Include(p => p.Tour)
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.Active, cancellationToken);
        if (property == null)
            return ApiResponse<PropertyDetailForBuyerDto>.ErrorResponse("Property not found.", code: ErrorCodes.PropertyNotFound);

        var hasListing = await dbContext.PropertyListings
            .AnyAsync(l => l.PropertyId == propertyId && l.IsActive, cancellationToken);
        if (!hasListing)
            return ApiResponse<PropertyDetailForBuyerDto>.ErrorResponse("Property not found.", code: ErrorCodes.PropertyNotFound);

        var listings = await dbContext.PropertyListings
            .AsNoTracking()
            .Include(l => l.Currency)
            .Where(l => l.PropertyId == propertyId && l.IsActive)
            .Select(l => new ListingLineDto
            {
                ListingId = l.Id,
                ListingType = l.ListingType,
                Price = l.Price,
                CurrencyCode = l.Currency.Code,
                CurrencySymbol = l.Currency.Symbol
            })
            .ToListAsync(cancellationToken);

        var dto = new PropertyDetailForBuyerDto
        {
            Property = ToResponse(property),
            TourShareToken = property.Tour?.ShareToken,
            Listings = listings
        };
        return ApiResponse<PropertyDetailForBuyerDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<bool>> Delete(int propertyId, int ownerId, CancellationToken cancellationToken = default)
    {
        var property = await dbContext.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && p.OwnerId == ownerId, cancellationToken);
        if (property == null)
            return ApiResponse<bool>.ErrorResponse("Property not found.", code: ErrorCodes.PropertyNotFound);
        dbContext.Properties.Remove(property);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    private static PropertyResponse ToResponse(Property p)
    {
        return new PropertyResponse
        {
            Id = p.Id,
            Category = p.Category,
            Lat = p.Lat,
            Lng = p.Lng,
            Description = p.Description,
            Area = p.Area,
            Price = p.Price,
            Active = p.Active,
            TourId = p.TourId,
            TourTitle = p.Tour?.Title,
            Address = p.Address?.Address,
            Locality = p.Address?.Locality,
            AdministrativeArea = p.Address?.AdministrativeArea,
            CountryCode = p.Address?.CountryCode,
            PostalCode = p.Address?.PostalCode,
            CreatedAt = p.CreatedAt
        };
    }
}
