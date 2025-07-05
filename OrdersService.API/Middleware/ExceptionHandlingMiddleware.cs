namespace eCommerce.OrdersService.API.Middleware;

// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class ExceptionHandlingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<ExceptionHandlingMiddleware> _logger;

  public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task Invoke(HttpContext httpContext)
  {
    try
    {
      await _next(httpContext);
    }
    catch (Exception ex)
    {
      if(ex.InnerException is not null)
      {
        _logger.LogError($"{ex.InnerException.GetType()}: {ex.InnerException.Message}");
      }

      _logger.LogError($"{ex.GetType().ToString()}: {ex.Message}");

      httpContext.Response.StatusCode = 500;
      await httpContext.Response.WriteAsJsonAsync(new
      {
        ex.Message,
        Type = ex.Message.GetType().ToString()
      });
    }
  }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class ExceptionHandlingMiddlewareExtensions
{
  public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
  {
    return builder.UseMiddleware<ExceptionHandlingMiddleware>();
  }
}
