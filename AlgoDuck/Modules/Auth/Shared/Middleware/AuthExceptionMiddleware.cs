using System.Net;
using System.Text.Json;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Shared.Http;
using FluentValidationException = FluentValidation.ValidationException;
using AuthValidationException = AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException;

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
            await WriteAuthErrorAsync(context, ex.Message, GetStatusCode(ex), ex.Code);
        }
        catch (FluentValidationException ex)
        {
            var msg = ex.Errors?.Select(e => e.ErrorMessage).FirstOrDefault() ?? "Validation failed.";
            _logger.LogWarning(ex, "Validation error.");
            await WriteAuthErrorAsync(context, msg, HttpStatusCode.BadRequest, "validation_error");
        }
    }

    private static async Task WriteAuthErrorAsync(HttpContext context, string message, HttpStatusCode status, string code)
    {
        var response = new StandardApiResponse
        {
            Status = Status.Error,
            Message = message
        };

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        context.Response.Headers["X-Auth-Error"] = code;

        var payload = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(payload);
    }

    private static HttpStatusCode GetStatusCode(AuthException exception)
    {
        if (exception is PermissionException) return HttpStatusCode.Forbidden;
        if (exception is AuthValidationException) return HttpStatusCode.BadRequest;
        if (exception is EmailVerificationException or TwoFactorException or TokenException) return HttpStatusCode.Unauthorized;
        if (exception is ApiKeyException) return HttpStatusCode.Unauthorized;
        return HttpStatusCode.BadRequest;
    }
}
