using Howsee.Application.DTOs.requests.Auth;
using Howsee.Application.DTOs.responses.Auth;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Domain.Entities;

namespace Howsee.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<ApiResponse<RegisterResponse>> Register(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<LoginResponse>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> Logout(LogoutRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> LogoutAllDevices(CancellationToken cancellationToken = default);
    Task<ApiResponse<User>> Me(CancellationToken cancellationToken = default);

    Task<ApiResponse<object>> SendVerificationAsync(SendVerificationRequest request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequest request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
