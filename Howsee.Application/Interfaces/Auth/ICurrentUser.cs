using Howsee.Domain.Enums;

namespace Howsee.Application.Interfaces.Auth;

public interface ICurrentUser
{
    int Id { get; }
    UserRole Role { get; }
}
