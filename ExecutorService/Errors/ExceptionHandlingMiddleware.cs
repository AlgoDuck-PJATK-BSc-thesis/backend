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
        
        // var response = exception switch
        // {
        //     CompilationHandlerChannelReadException err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.InternalServerError, ErrMsg = "The service you tried to use is temporarily unavailable, please try again later" },
        //     FileNotFoundException _ => new ExecutorErrorResponse { StatusCode = HttpStatusCode.InternalServerError, ErrMsg = "Something went wrong during code execution. Please try again later" },
        //     CompilationException err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.BadRequest, ErrMsg = err.Message },
        //     AmazonS3Exception err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.InternalServerError, ErrMsg = err.Message },
        //     VmQueryTimedOutException err => new ExecutorErrorResponse { StatusCode = HttpStatusCode.BadRequest, ErrMsg = "query timed out" },
        //     _ => new ExecutorErrorResponse { StatusCode = HttpStatusCode.InternalServerError, ErrMsg = "Internal server error" },
        // };

        // context.Response.ContentType = "application/json";
        // context.Response.StatusCode = (int)response.StatusCode;
        await context.Response.WriteAsJsonAsync(new{});
    }
}