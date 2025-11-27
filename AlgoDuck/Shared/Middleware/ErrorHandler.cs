using System.Net;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Http;

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
            response.ContentType = "application/json";

            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "Unexpected error";
            var code = "internal_error";

            if (error is AppException appEx)
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
                Message = message
            });
        }
    }
}