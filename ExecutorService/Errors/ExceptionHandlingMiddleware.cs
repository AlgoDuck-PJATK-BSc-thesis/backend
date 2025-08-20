using System.Net;
using Amazon.S3;
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
        logger.LogError(exception, "An unexpected error occurred.");
        
        var response = exception switch
        {
            FunctionSignatureException _ => new ExceptionReponseDto(HttpStatusCode.BadRequest, "critical function signature modified. Exiting"),
            JavaSyntaxException err => new ExceptionReponseDto(HttpStatusCode.BadRequest, $"java syntax error: {err.Message}"),
            EntrypointNotFoundException err => new ExceptionReponseDto(HttpStatusCode.BadRequest, err.Message),
            TemplateModifiedException err => new ExceptionReponseDto(HttpStatusCode.BadRequest, err.Message),
            EmptyProgramException ex => new ExceptionReponseDto(HttpStatusCode.OK, ex.Message),
            TemplateParsingException err => new ExceptionReponseDto(HttpStatusCode.InternalServerError, "The service you tried to use is temporarily unavailable, please try again later"),
            UnknownCompilationException err => new ExceptionReponseDto(HttpStatusCode.InternalServerError, "The service you tried to use is temporarily unavailable, please try again later"),
            CompilationHandlerChannelReadException err => new ExceptionReponseDto(HttpStatusCode.InternalServerError, "The service you tried to use is temporarily unavailable, please try again later"),
            LanguageException err => new ExceptionReponseDto(HttpStatusCode.BadRequest, err.Message),
            FileNotFoundException _ => new ExceptionReponseDto(HttpStatusCode.InternalServerError, "Something went wrong during code execution. Please try again later"),
            CompilationException err => new ExceptionReponseDto(HttpStatusCode.BadRequest, err.Message),
            AmazonS3Exception err => new ExceptionReponseDto(HttpStatusCode.InternalServerError, err.Message),
            _ => new ExceptionReponseDto(HttpStatusCode.InternalServerError, "Internal server error"),
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}