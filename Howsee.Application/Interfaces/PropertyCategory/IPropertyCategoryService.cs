using Howsee.Application.DTOs.requests.PropertyCategory;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.PropertyCategory;

namespace Howsee.Application.Interfaces.PropertyCategory;

public interface IPropertyCategoryService
{
    Task<IReadOnlyList<PropertyCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PropertyCategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PropertyCategoryDto>> CreateAsync(CreatePropertyCategoryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PropertyCategoryDto>> UpdateAsync(int id, UpdatePropertyCategoryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
