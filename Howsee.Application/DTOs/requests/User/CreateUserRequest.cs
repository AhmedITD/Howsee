using Howsee.Domain.Enums;

namespace Howsee.Application.DTOs.requests.User;

public class CreateUserRequest
{
    public required string FullName { get; set; }
    public required string Phone { get; set; }
    public required string Password { get; set; }
    public UserRole Role { get; set; } = UserRole.Buyer;
}
