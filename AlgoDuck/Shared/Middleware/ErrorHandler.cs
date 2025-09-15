using System.Net;
using AlgoDuck.Shared.Exceptions;

namespace AlgoDuck.Shared.Middleware;

public class ErrorHandler
{
    private readonly RequestDelegate _next;
    public ErrorHandler(RequestDelegate next)
    {
        _next = next;
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

            int statusCode = (int)HttpStatusCode.InternalServerError;
            string message = "An unexpected error occurred.";

            if (error is AppException appEx)
            {
                statusCode = appEx.StatusCode;
                message = appEx.Message;
            }

            response.StatusCode = statusCode;

            var result = System.Text.Json.JsonSerializer.Serialize(new { error = message });
            await response.WriteAsync(result);
        }
    }
}
