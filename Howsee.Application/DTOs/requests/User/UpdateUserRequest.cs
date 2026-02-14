using Howsee.Domain.Enums;

namespace Howsee.Application.DTOs.requests.User;

public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public UserRole? Role { get; set; }
    public string? NewPassword { get; set; }
}
