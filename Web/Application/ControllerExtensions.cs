using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Web.Application;

public abstract class BaseController : ControllerBase {
  protected ActionResult<T> OkResult<T>(T value) {
    return Ok(value);
  }
  
  protected ActionResult<T?> OkResultNullable<T>(T? value) where T : class {
    return Ok(value);
  }

  protected record class UserInfo(int Id, string Name);
  
  protected UserInfo? GetUserInfo() {
    var userIdExist = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
    var userName = User.FindFirst(ClaimTypes.Name)?.Value;
    
    if (!userIdExist || userName == null) return null;

    var userInfo = new UserInfo(userId, userName);
    return userInfo;
  }
}