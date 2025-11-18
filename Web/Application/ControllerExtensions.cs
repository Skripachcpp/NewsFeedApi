using Microsoft.AspNetCore.Mvc;

namespace Web.Application;

public abstract class BaseController : ControllerBase {
  protected ActionResult<T> OkResult<T>(T value) {
    return Ok(value);
  }
  
  protected ActionResult<T?> OkNullableResult<T>(T? value) where T : class {
    return Ok(value);
  }
}