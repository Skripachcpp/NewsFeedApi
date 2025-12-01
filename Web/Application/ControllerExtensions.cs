using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Web.Application;

public abstract class BaseController : ControllerBase
{
    protected ActionResult<T> OkResult<T>(T value)
    {
        return this.Ok(value);
    }

    protected sealed record UserInfo(int id, string name);

    protected UserInfo? GetUserInfo()
    {
        var userIdExist = int.TryParse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
        var userName = this.User.FindFirst(ClaimTypes.Name)?.Value;

        if (!userIdExist || userName is null)
            return null;

        var userInfo = new UserInfo(userId, userName);
        return userInfo;
    }
}