using Howsee.Application.DTOs.requests.User;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.User;
using Howsee.Domain.Enums;

namespace Howsee.Application.Interfaces.Users;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserDto>> ListAsync(UserRole? role, string? search, bool includeDeleted, CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
