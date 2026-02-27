using Howsee.Application.Common;
using Howsee.Application.DTOs.requests.PropertyCategory;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.PropertyCategory;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.PropertyCategory;
using Howsee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Howsee.Application.Services;

public class PropertyCategoryService(IHowseeDbContext dbContext) : IPropertyCategoryService
{
    public async Task<IReadOnlyList<PropertyCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.PropertyCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new PropertyCategoryDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PropertyCategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.PropertyCategories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new PropertyCategoryDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ApiResponse<PropertyCategoryDto>> CreateAsync(CreatePropertyCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        if (await dbContext.PropertyCategories.AnyAsync(c => c.Name == name, cancellationToken))
            return ApiResponse<PropertyCategoryDto>.ErrorResponse("A category with this name already exists.", code: ErrorCodes.PropertyCategoryNameExists);

        var category = new PropertyCategoryLookup { Name = name };
        dbContext.PropertyCategories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = await GetByIdAsync(category.Id, cancellationToken);
        return ApiResponse<PropertyCategoryDto>.SuccessResponse(dto!);
    }

    public async Task<ApiResponse<PropertyCategoryDto>> UpdateAsync(int id, UpdatePropertyCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await dbContext.PropertyCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category == null)
            return ApiResponse<PropertyCategoryDto>.ErrorResponse("Property category not found.", code: ErrorCodes.PropertyCategoryNotFound);

        if (request.Name != null)
        {
            var name = request.Name.Trim();
            if (await dbContext.PropertyCategories.AnyAsync(c => c.Id != id && c.Name == name, cancellationToken))
                return ApiResponse<PropertyCategoryDto>.ErrorResponse("A category with this name already exists.", code: ErrorCodes.PropertyCategoryNameExists);
            category.Name = name;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = await GetByIdAsync(category.Id, cancellationToken);
        return ApiResponse<PropertyCategoryDto>.SuccessResponse(dto!);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await dbContext.PropertyCategories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category == null)
            return ApiResponse<bool>.ErrorResponse("Property category not found.", code: ErrorCodes.PropertyCategoryNotFound);

        dbContext.PropertyCategories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true);
    }
}
