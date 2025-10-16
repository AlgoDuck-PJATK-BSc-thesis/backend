using System.Net;
using AlgoDuckShared.Executor.SharedTypes;
using Amazon.S3;
using ExecutorService.Analyzer._AnalyzerUtils;
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
            FunctionSignatureException _ => new ExecutorErrorResponse { StatusCode = HttpStatusCode.BadRequest, ErrMsg = "critical function signature modified. Exiting" },
            JavaSyntaxException err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.BadRequest, ErrMsg = $"java syntax error: {err.Message}" },
            EntrypointNotFoundException err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.BadRequest, ErrMsg = err.Message },
            TemplateModifiedException err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.BadRequest, ErrMsg = err.Message },
            EmptyProgramException ex => new ExecutorErrorResponse { StatusCode = HttpStatusCode.OK, ErrMsg = ex.Message },
            TemplateParsingException err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.InternalServerError, ErrMsg = "The service you tried to use is temporarily unavailable, please try again later" },
            UnknownCompilationException err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.InternalServerError, ErrMsg = "The service you tried to use is temporarily unavailable, please try again later" },
            CompilationHandlerChannelReadException err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.InternalServerError, ErrMsg = "The service you tried to use is temporarily unavailable, please try again later" },
            LanguageException err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.BadRequest, ErrMsg = err.Message },
            FileNotFoundException _ => new ExecutorErrorResponse { StatusCode = HttpStatusCode.InternalServerError, ErrMsg = "Something went wrong during code execution. Please try again later" },
            CompilationException err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.BadRequest, ErrMsg = err.Message },
            MangledControlSymbolException err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.InternalServerError, ErrMsg = "" },
            AmazonS3Exception err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.InternalServerError, ErrMsg = err.Message },
            VmQueryTimedOutException err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.BadRequest, ErrMsg = "query timed out" },
            _ => new ExecutorErrorResponse { StatusCode = HttpStatusCode.InternalServerError, ErrMsg = "Internal server error" },
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}