using System.Net;
using System.Text.Json;
using ExecutorService.Errors.Exceptions;

namespace ExecutorService.Errors;

public class ErrorResponse
{
    public string TraceId { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string Error { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        
        var (statusCode, error, message) = MapException(exception);
        
        LogException(exception, traceId, statusCode);
        
        var response = new ErrorResponse
        {
            TraceId = traceId,
            StatusCode = (int)statusCode,
            Error = error,
            Message = message,
            Details = environment.IsDevelopment() ? exception.ToString() : null
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }

    private static (HttpStatusCode statusCode, string error, string message) MapException(Exception exception)
    {
        return exception switch
        {
            CompilationException ex => (
                HttpStatusCode.BadRequest, 
                "COMPILATION_ERROR", 
                ex.Message),
            
            VmQueryTimedOutException => (
                HttpStatusCode.RequestTimeout, 
                "EXECUTION_TIMEOUT", 
                "Code execution timed out. Your code may have an infinite loop or be too slow."),
            
            ArgumentException ex => (
                HttpStatusCode.BadRequest, 
                "INVALID_ARGUMENT", 
                ex.Message),
            
            VmClusterOverloadedException => (
                HttpStatusCode.ServiceUnavailable, 
                "SERVICE_OVERLOADED", 
                "The execution service is currently at capacity. Please try again in a moment."),
            
            CompilationHandlerChannelReadException => (
                HttpStatusCode.ServiceUnavailable, 
                "SERVICE_UNAVAILABLE", 
                "The compilation service is temporarily unavailable. Please try again later."),
            
            ExecutionOutputNotFoundException => (
                HttpStatusCode.InternalServerError, 
                "EXECUTION_ERROR", 
                "Code execution completed but output could not be retrieved."),
            
            OperationCanceledException => (
                HttpStatusCode.ServiceUnavailable, 
                "REQUEST_CANCELLED", 
                "The request was cancelled."),
            
            TimeoutException ex => (
                HttpStatusCode.GatewayTimeout, 
                "TIMEOUT", 
                ex.Message),
            
            _ => (
                HttpStatusCode.InternalServerError, 
                "INTERNAL_ERROR", 
                "An unexpected error occurred. Please try again later.")
        };
    }

    private void LogException(Exception exception, string traceId, HttpStatusCode statusCode)
    {
        var level = (int)statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
        
        logger.Log(
            level,
            exception,
            "Request {TraceId} failed with {StatusCode}: {ExceptionType} - {Message}",
            traceId,
            (int)statusCode,
            exception.GetType().Name,
            exception.Message);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}