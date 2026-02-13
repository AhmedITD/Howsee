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

    public async Task<ApiResponse<List<PropertyResponse>>> List(int ownerId, bool? active = null, PropertyCategory? category = null, int? tourId = null, CancellationToken cancellationToken = default)
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
