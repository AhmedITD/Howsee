using Microsoft.AspNetCore.Mvc;
using Howsee.Application.Interfaces.Auth;

namespace Howsee.Api.Common;

public abstract class BaseController : ControllerBase
{
    /// <summary>Returns Unauthorized if current user is not set; otherwise null so the action can proceed.</summary>
    // protected ActionResult? RequireUserId(ICurrentUser currentUser) =>
    //     currentUser.Id == 0 ? Unauthorized() : null;
}
