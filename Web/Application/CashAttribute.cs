using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Web.Application;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class CashAttribute(string keyPart) : ActionFilterAttribute {
  private string SerializeToKeyValueString(object? value) {
    if (value == null) return "null";
    
    var properties = value.GetType().GetProperties();
    var parts = new List<string>();
    
    foreach (var prop in properties) {
      var propValue = prop.GetValue(value);
      var valueStr = propValue?.ToString() ?? "null";
      
      valueStr = valueStr.Replace(" ", "_").Replace("_", "");
        
      parts.Add($"{prop.Name}_{valueStr}");
    }
    
    return string.Join("_", parts);
  }
  
  private string GetQueryParams(string keyPart, ActionExecutingContext context) {
    var queryString = context.HttpContext.Request.QueryString.ToString();
    var parameters = keyPart;
    foreach (var argument in context.ActionArguments) {
      var paramName = argument.Key;
      var paramValue = argument.Value;
      
      if (paramValue is CancellationToken) continue;
      
      parameters += SerializeToKeyValueString(paramValue);
    }

    return parameters;
  }
  
  
  public override async Task OnActionExecutionAsync(
    ActionExecutingContext context,
    ActionExecutionDelegate next) {
    
    var cancellationToken = context.HttpContext.RequestAborted;
    var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();

    var key = GetQueryParams(keyPart, context);
    
    var cachedJson = await cache.GetStringAsync(key, cancellationToken);
    if (cachedJson != null) {
      // return new OkObjectResult(cachedJson);
    }
    
    await next();
  }
  
  public override void OnResultExecuting(ResultExecutingContext context) {
    // Перехватываем Response.Body до записи результата
    var originalBody = context.HttpContext.Response.Body;
    var memoryStream = new MemoryStream();
    context.HttpContext.Response.Body = memoryStream;
    
    // Сохраняем MemoryStream и оригинальный поток в HttpContext.Items
    context.HttpContext.Items["CashAttribute_MemoryStream"] = memoryStream;
    context.HttpContext.Items["CashAttribute_OriginalBody"] = originalBody;
  }
}

