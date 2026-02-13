using Howsee.Domain.Entities;

namespace Howsee.Application.Interfaces.Auth;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
