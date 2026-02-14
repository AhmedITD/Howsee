using Howsee.Application.Common;
using Howsee.Application.DTOs.requests.User;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.User;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Auth;
using Howsee.Application.Interfaces.Users;
using Howsee.Domain.Entities;
using Howsee.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Howsee.Application.Services;

public class UserManagementService(
    IHowseeDbContext dbContext,
    IPasswordService passwordService) : IUserManagementService
{
    public async Task<IReadOnlyList<UserDto>> ListAsync(UserRole? role, string? search, bool includeDeleted, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Users.AsQueryable();
        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        if (role.HasValue)
            query = query.Where(u => u.Role == role.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(term) ||
                u.Phone.ToLower().Contains(term));
        }

        return await query
            .OrderBy(u => u.FullName)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Phone = u.Phone,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                IsDeleted = u.IsDeleted,
                DeletedAt = u.DeletedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .IgnoreQueryFilters()
            .Where(u => u.Id == id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Phone = u.Phone,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                IsDeleted = u.IsDeleted,
                DeletedAt = u.DeletedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ApiResponse<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var phone = request.Phone?.Trim();
        if (string.IsNullOrEmpty(phone))
            return ApiResponse<UserDto>.ErrorResponse("Phone is required.", code: ErrorCodes.ValidationFailed);

        var exists = await dbContext.Users.AnyAsync(u => u.Phone == phone, cancellationToken);
        if (exists)
            return ApiResponse<UserDto>.ErrorResponse("Phone number already registered.", code: ErrorCodes.PhoneAlreadyExists);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Phone = phone,
            PasswordHash = passwordService.HashPassword(request.Password),
            Role = request.Role
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = await GetByIdAsync(user.Id, cancellationToken);
        return ApiResponse<UserDto>.SuccessResponse(dto!);
    }

    public async Task<ApiResponse<UserDto>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null)
            return ApiResponse<UserDto>.ErrorResponse("User not found.", code: ErrorCodes.UserNotFound);

        if (request.FullName != null)
            user.FullName = request.FullName.Trim();

        if (request.Phone != null)
        {
            var phone = request.Phone.Trim();
            if (string.IsNullOrEmpty(phone))
                return ApiResponse<UserDto>.ErrorResponse("Phone cannot be empty.", code: ErrorCodes.ValidationFailed);
            var exists = await dbContext.Users.AnyAsync(u => u.Phone == phone && u.Id != id, cancellationToken);
            if (exists)
                return ApiResponse<UserDto>.ErrorResponse("Phone number already in use.", code: ErrorCodes.PhoneAlreadyExists);
            user.Phone = phone;
        }

        if (request.Role.HasValue)
            user.Role = request.Role.Value;

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
            user.PasswordHash = passwordService.HashPassword(request.NewPassword);

        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = await GetByIdAsync(user.Id, cancellationToken);
        return ApiResponse<UserDto>.SuccessResponse(dto!);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null)
            return ApiResponse<bool>.ErrorResponse("User not found.", code: ErrorCodes.UserNotFound);

        if (user.IsDeleted)
            return ApiResponse<bool>.SuccessResponse(true);

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true);
    }
}
