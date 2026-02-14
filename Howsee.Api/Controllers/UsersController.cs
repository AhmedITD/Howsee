using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Howsee.Api.Common;
using Howsee.Application.Common;
using Howsee.Application.DTOs.requests.User;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.User;
using Howsee.Application.Interfaces.Users;
using Howsee.Domain.Enums;

namespace Howsee.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = nameof(UserRole.Administrator))]
public class UsersController(IUserManagementService userManagementService) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> List(
        [FromQuery] UserRole? role,
        [FromQuery] string? search,
        [FromQuery] bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var users = await userManagementService.ListAsync(role, search, includeDeleted, cancellationToken);
        return Ok(ApiResponse<List<UserDto>>.SuccessResponse(users.ToList()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var user = await userManagementService.GetByIdAsync(id, cancellationToken);
        return user == null
            ? NotFound(ApiResponse<UserDto>.ErrorResponse("User not found.", code: ErrorCodes.UserNotFound))
            : Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var result = await userManagementService.CreateAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var result = await userManagementService.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result) : result.Code == ErrorCodes.UserNotFound ? NotFound(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id, CancellationToken cancellationToken = default)
    {
        var result = await userManagementService.DeleteAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
