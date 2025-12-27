using System.Net;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Http;

using FluentValidationException = FluentValidation.ValidationException;
using UserValidationException = AlgoDuck.Modules.User.Shared.Exceptions.ValidationException;

namespace AlgoDuck.Shared.Middleware;

public class ErrorHandler
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
            var response = context.Response;

            if (response.HasStarted)
            {
                throw;
            }

            response.ContentType = "application/json; charset=utf-8";

            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "Unexpected error";
            var code = "internal_error";
            object? body = null;

            if (error is FluentValidationException validationEx)
            {
                statusCode = StatusCodes.Status400BadRequest;
                message = "Validation failed.";
                code = "validation_error";

                body = validationEx.Errors
                    .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "general" : e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().ToArray()
                    );
            }
            else if (error is UserValidationException userValidationEx)
            {
                statusCode = StatusCodes.Status400BadRequest;
                message = string.IsNullOrWhiteSpace(userValidationEx.Message) ? "Validation failed." : userValidationEx.Message;
                code = "validation_error";
            }
            else if (error is AppException appEx)
            {
                statusCode = appEx.StatusCode;
                message = appEx.Message;
                code = appEx.GetType().Name.Replace("Exception", "").ToLowerInvariant();
            }

            _logger.LogError(error, "Unhandled exception: {Code} {Message}", code, message);

            response.StatusCode = statusCode;

            await response.WriteAsJsonAsync(new StandardApiResponse
            {
                Status = Status.Error,
                Message = message,
                Body = body
            });
        }
    }
}