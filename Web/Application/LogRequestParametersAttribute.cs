using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Web.Application;

/// <summary>
/// Атрибут для логирования параметров запроса к API в консоль
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class LogRequestParametersAttribute : ActionFilterAttribute {
  public override void OnActionExecuting(ActionExecutingContext context) {
    var logger = context.HttpContext.RequestServices
      .GetRequiredService<ILogger<LogRequestParametersAttribute>>();
    
    var actionName = context.ActionDescriptor.DisplayName ?? "Unknown";
    var method = context.HttpContext.Request.Method;
    var path = context.HttpContext.Request.Path;
    var queryString = context.HttpContext.Request.QueryString.ToString();
    
    // Собираем все параметры action метода
    var parameters = new Dictionary<string, object?>();
    
    foreach (var argument in context.ActionArguments) {
      var paramName = argument.Key;
      var paramValue = argument.Value;
      
      // Пропускаем CancellationToken, так как он не информативен
      if (paramValue is CancellationToken) continue;
      
      // Сериализуем значение для красивого вывода
      string valueString;
      try {
        if (paramValue == null) {
          valueString = "null";
        } else if (paramValue is string || paramValue.GetType().IsPrimitive) {
          valueString = paramValue.ToString() ?? "null";
        } else {
          valueString = JsonSerializer.Serialize(paramValue, new JsonSerializerOptions {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
          });
        }
      } catch {
        valueString = paramValue?.ToString() ?? "null";
      }
      
      parameters[paramName] = valueString;
    }
    
    // Логируем в консоль
    var parametersJson = parameters.Count > 0
      ? JsonSerializer.Serialize(parameters, new JsonSerializerOptions {
          WriteIndented = false,
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })
      : "{}";
    
    var logMessage = $"[API Request] {method} {path}";
    if (!string.IsNullOrEmpty(queryString)) {
      logMessage += $" | Query: {queryString}";
    }
    logMessage += $" | Action: {actionName} | Parameters: {parametersJson}";
    
    logger.LogInformation(logMessage);
    
    base.OnActionExecuting(context);
  }
}

