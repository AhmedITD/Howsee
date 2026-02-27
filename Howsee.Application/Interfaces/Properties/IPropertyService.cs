using Howsee.Application.DTOs.requests.Property;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Property;
using Howsee.Domain.Enums;

namespace Howsee.Application.Interfaces.Properties;

public interface IPropertyService
{
    Task<ApiResponse<PropertyResponse>> Create(CreatePropertyRequest request, int ownerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PropertyResponse>> Update(int propertyId, UpdatePropertyRequest request, int ownerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PropertyResponse>> Get(int propertyId, int ownerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PropertyResponse>>> List(int ownerId, bool? active = null, Howsee.Domain.Enums.PropertyCategory? category = null, int? tourId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> Delete(int propertyId, int ownerId, CancellationToken cancellationToken = default);
}
