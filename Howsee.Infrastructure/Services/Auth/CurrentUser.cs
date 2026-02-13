using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Howsee.Application.Interfaces.Auth;
using Howsee.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Howsee.Infrastructure.Services.Auth;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public int Id
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return int.TryParse(sub, out var id) ? id : 0;
        }
    }

    public UserRole Role
    {
        get
        {
            var roleClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(roleClaim, true, out var role) ? role : UserRole.Buyer;
        }
    }
}
