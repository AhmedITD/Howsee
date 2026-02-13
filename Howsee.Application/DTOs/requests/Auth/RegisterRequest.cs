namespace Howsee.Application.DTOs.requests.Auth;

public class RegisterRequest
{
    public required string FullName { get; set; }
    public required string Phone { get; set; }
    public required string Password { get; set; }
    public required string VerificationCode { get; set; }
    public string? ImageUrl { get; set; }
}
