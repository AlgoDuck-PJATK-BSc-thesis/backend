using System.Net;
using System.Text.Json;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Auth.Shared.Middleware;

public sealed class AuthExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthExceptionMiddleware> _logger;

    public AuthExceptionMiddleware(RequestDelegate next, ILogger<AuthExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AuthException ex)
        {
            _logger.LogWarning(ex, "Auth error with code {Code}.", ex.Code);
            await WriteAuthErrorAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error.");
            await WriteUnexpectedErrorAsync(context);
        }
    }

    private static async Task WriteAuthErrorAsync(HttpContext context, AuthException exception)
    {
        var statusCode = GetStatusCode(exception);
        var response = new StandardApiResponse
        {
            Status = Status.Error,
            Message = exception.Message
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        context.Response.Headers["X-Auth-Error"] = exception.Code;

        var payload = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(payload);
    }

    private static async Task WriteUnexpectedErrorAsync(HttpContext context)
    {
        var response = new StandardApiResponse
        {
            Status = Status.Error,
            Message = "An unexpected error occurred."
        };

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(payload);
    }

    private static HttpStatusCode GetStatusCode(AuthException exception)
    {
        if (exception is PermissionException)
        {
            return HttpStatusCode.Forbidden;
        }

        if (exception is ValidationException)
        {
            return HttpStatusCode.BadRequest;
        }

        if (exception is EmailVerificationException or TwoFactorException or TokenException)
        {
            return HttpStatusCode.Unauthorized;
        }

        if (exception is ApiKeyException)
        {
            return HttpStatusCode.Unauthorized;
        }

        return HttpStatusCode.BadRequest;
    }
}