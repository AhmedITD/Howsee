using System.ComponentModel.DataAnnotations;

namespace Howsee.Application.DTOs.requests.Auth;

public class LogoutRequest
{
    [Required]
    public required string RefreshToken { get; set; }
}
