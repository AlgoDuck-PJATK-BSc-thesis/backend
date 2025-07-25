using System.Net;
using ExecutorService.Errors.Exceptions;

namespace ExecutorService.Errors;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
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
        // logger.LogError(exception, "An unexpected error occurred.");
        
        var response = exception switch
        {
            FunctionSignatureException _ => new ExceptionReponseDto(HttpStatusCode.BadRequest, "critical function signature modified. Exiting"),
            JavaSyntaxException err => new ExceptionReponseDto(HttpStatusCode.BadRequest, $"java syntax error: {err.Message}"),
            LanguageException err => new ExceptionReponseDto(HttpStatusCode.BadRequest, err.Message),
            FileNotFoundException _ => new ExceptionReponseDto(HttpStatusCode.InternalServerError, "Something went wrong during code execution. Please try again later"),
            CompilationException err => new ExceptionReponseDto(HttpStatusCode.BadRequest, err.Message),
            _ => new ExceptionReponseDto(HttpStatusCode.InternalServerError, "Internal server error"),
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}