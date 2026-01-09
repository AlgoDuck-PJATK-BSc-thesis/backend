using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Http;
using FluentValidationException = FluentValidation.ValidationException;

namespace AlgoDuck.Shared.Middleware;

public sealed class ErrorHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandler> _logger;

    public ErrorHandler(RequestDelegate next, ILogger<ErrorHandler> logger)
    {
        _next = next;
        _logger = logger;
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
            else if (error is AppException appEx)
            {
                statusCode = appEx.StatusCode;
                message = appEx.Message;
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
