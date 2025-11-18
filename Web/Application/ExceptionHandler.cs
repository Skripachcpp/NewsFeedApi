namespace Web.Application;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class ExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<ExceptionHandler> logger) : IExceptionHandler {
  public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken) {
    logger.LogError(exception, exception.Message);
    
    var context = new ProblemDetailsContext() {
      HttpContext = httpContext,
      Exception = exception,
      
      ProblemDetails = new ProblemDetails() {
        Title = exception.Message, // потенциально дыра
        Status = 500
      },
    };
    
    await problemDetailsService.WriteAsync(context);
    return true;
  }
}