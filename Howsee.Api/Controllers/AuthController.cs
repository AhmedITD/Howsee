using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Howsee.Api.Common;
using Howsee.Application.Common;
using Howsee.Application.DTOs.requests.Auth;
using Howsee.Application.DTOs.responses.Auth;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.Interfaces.Auth;
using Howsee.Domain.Entities;

namespace Howsee.Api.Controllers;

[Route("auth")]
public class AuthController(IAuthService authService, IValidator<RegisterRequest> registerValidator) : BaseController
{
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<RegisterResponse>.ErrorResponse("Validation failed.",
                validation.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()!),
                ErrorCodes.ValidationFailed));
        var result = await authService.Register(request, cancellationToken);
        return result.Success ? Created($"/auth/me", result) : BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        var result = await authService.Login(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // [HttpPost("refresh")]
    // public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
    // {
    //     var result = await authService.Refresh(request, cancellationToken);
    //     return result.Success ? Ok(result) : BadRequest(result);
    // }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var result = await authService.Logout(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("logoutAllDevices")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> LogoutAllDevices(CancellationToken cancellationToken = default)
    {
        var result = await authService.LogoutAllDevices(cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<User>>> Me(CancellationToken cancellationToken = default)
    {
        var response = await authService.Me(cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("send-verification")]
    public async Task<ActionResult<ApiResponse<object>>> SendVerification([FromBody] SendVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await authService.SendVerificationAsync(request, ipAddress, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await authService.ForgotPasswordAsync(request, ipAddress, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await authService.ResetPasswordAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
