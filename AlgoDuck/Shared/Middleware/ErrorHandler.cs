using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Http;
using FluentValidationException = FluentValidation.ValidationException;

namespace AlgoDuck.Shared.Middleware;

public sealed class ErrorHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandler> _logger;
    private readonly IWebHostEnvironment _env;

    public ErrorHandler(RequestDelegate next, ILogger<ErrorHandler> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.ContentType = "application/json; charset=utf-8";

            var statusCode = StatusCodes.Status500InternalServerError;
            var message = "Unexpected error";
            object? body = null;

            if (error is FluentValidationException fv)
            {
                statusCode = StatusCodes.Status400BadRequest;
                message = "Validation failed.";

                body = fv.Errors
                    .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "general" : e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage)
                              .Where(m => !string.IsNullOrWhiteSpace(m))
                              .Distinct()
                              .ToArray()
                    );
            }
            else if (error is PermissionException pex)
            {
                statusCode = StatusCodes.Status403Forbidden;
                message = pex.Message;
            }
            else if (error is TokenException tex)
            {
                var msg = tex.Message;
                var m = msg.ToLowerInvariant();

                if (m.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    statusCode = StatusCodes.Status404NotFound;
                }
                else if (m.Contains("invalid", StringComparison.OrdinalIgnoreCase))
                {
                    statusCode = StatusCodes.Status400BadRequest;
                }
                else if (m.Contains("not authenticated", StringComparison.OrdinalIgnoreCase) ||
                         m.Contains("unauthenticated", StringComparison.OrdinalIgnoreCase) ||
                         m.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
                {
                    statusCode = StatusCodes.Status401Unauthorized;
                }
                else
                {
                    statusCode = StatusCodes.Status401Unauthorized;
                }

                message = msg;
                
            }
            else if (error is AppException appEx)
            {
                statusCode = appEx.StatusCode;
                message = appEx.Message;
            }

            if (statusCode == StatusCodes.Status500InternalServerError &&
                (_env.IsDevelopment() ||
                 string.Equals(_env.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(_env.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase)))
            {
                body = new
                {
                    Type = error.GetType().FullName,
                    error.Message,
                    error.StackTrace
                };
            }

            _logger.LogError(error, "Unhandled exception: {StatusCode} {Message}", statusCode, message);

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(new StandardApiResponse<object?>
            {
                Status = Status.Error,
                Message = message,
                Body = body
            });
        }
    }
}
