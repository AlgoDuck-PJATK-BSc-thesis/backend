using System.Data;
using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace AlgoDuck.Shared.Http;

public class Result<T, TE> : OneOfBase<T, TE>
{
    private Result(OneOf<T, TE> input) : base(input)
    {
    }

    public static Result<T, TE> Ok(T value) => new(value);
    public static Result<T, TE> Err(TE error) => new(error);

    
    public bool IsOk => IsT0;
    public bool IsErr => IsT1;
    public T? AsOk
    {
        get
        {
            try
            {
                return AsT0;
            }
            catch (Exception e)
            {
                return default;
            }
        }
    }
    public TE? AsErr
    {
        get
        {
            try
            {
                return AsT1;
            }
            catch (Exception e)
            {
                return default;
            }
        }
    }
}

public class ErrorObject<TE>
{
    public required ErrorType Type { get; set; }
    public required TE Body { get; set; }
    
    public static ErrorObject<TE> NotFound(TE body) => new() { Type = ErrorType.NotFound, Body = body };
    public static ErrorObject<TE> BadRequest(TE body) => new() { Type = ErrorType.BadRequest, Body = body };
    public static ErrorObject<TE> Unauthorized(TE body) => new() { Type = ErrorType.Unauthorized, Body = body };
    public static ErrorObject<TE> Forbidden(TE body) => new() { Type = ErrorType.Forbidden, Body = body };
    public static ErrorObject<TE> Conflict(TE body) => new() { Type = ErrorType.Conflict, Body = body };
    public static ErrorObject<TE> ValidationError(TE body) => new() { Type = ErrorType.ValidationError, Body = body };
    public static ErrorObject<TE> InternalError(TE body) => new() { Type = ErrorType.InternalError, Body = body };
}

public enum ErrorType
{
    NotFound,
    BadRequest,
    Unauthorized,
    Forbidden,
    Conflict,
    ValidationError,
    InternalError
}

public static class ResultHttpExtensions
{
    public static async Task<IActionResult> ToActionResultAsync<T, TE>(this Task<Result<T, ErrorObject<TE>>> resultTask, string message = "")
    {
        var result = await resultTask;
        return result.ToActionResult(message);
    } 
    
    public static IActionResult ToActionResult<T, TE>(
        this Result<T, ErrorObject<TE>> result, string message = "")
    {
        return result.Match(
            ok => new OkObjectResult(new StandardApiResponse<T>
            {
                Message = message,
                Body = ok
            }),
            err => err.Type switch
            {
                ErrorType.BadRequest or ErrorType.ValidationError => new BadRequestObjectResult(new StandardApiResponse<TE>
                {
                    Status = Status.Error,
                    Message = message,
                    Body = err.Body
                }),
                ErrorType.NotFound => new NotFoundObjectResult(new StandardApiResponse<TE>
                {
                    Status = Status.Error,
                    Message = message,
                    Body = err.Body
                }),
                ErrorType.Unauthorized => new UnauthorizedObjectResult(new StandardApiResponse<TE>
                {
                    Status = Status.Error,
                    Message = message,
                    Body = err.Body
                }),
                ErrorType.Forbidden => new ObjectResult(new StandardApiResponse<TE>
                {
                    Status = Status.Error,
                    Message = message,
                    Body = err.Body
                }) { StatusCode = 403 },
                ErrorType.Conflict => new ConflictObjectResult(new StandardApiResponse<TE>
                {
                    Status = Status.Error,
                    Message = message,
                    Body = err.Body
                }),
                _ => new ObjectResult(new StandardApiResponse<string>
                {
                    Status = Status.Error,
                    Message = message,
                    Body = "An unexpected error occurred"
                }) { StatusCode = 500 }
            }
        );
    }
}

public static class ResultToolingExtensions
{
    public static Result<TNew, TE> Bind<T, TNew, TE>(
        this Result<T, TE> result, 
        Func<T, Result<TNew, TE>> mapper)
    {
        return result.Match(
            mapper,
            Result<TNew, TE>.Err
        );
    }

    /*
     * binds to result if IsOk and executes a mapping function
     * short circuits and returns if result IsErr
     */
    public static async Task<Result<TNew, TE>> BindAsync<T, TNew, TE>(
        this Result<T, TE> result, 
        Func<T, Task<Result<TNew, TE>>> mapper)
    {
        return await result.Match(
            async ok => await mapper(ok),
            err => Task.FromResult(Result<TNew, TE>.Err(err))
        );
    }
    
    public static async Task<Result<TNew, TE>> BindResult<T, TNew, TE>(
        this Task<Result<T, TE>> resultTask, 
        Func<T, Result<TNew, TE>> mapper)
    {
        var result = await resultTask;
        return result.Match(
            ok => mapper(ok),
            err => Result<TNew, TE>.Err(err)
        );
    }
    
    

    public static async Task<Result<TNew, TE>> MapAsync<T, TNew, TE>(
        this Task<Result<T, TE>> resultTask,
        Func<T, Result<TNew, TE>> mapper)
    {
        var result = await resultTask;
        return result.Match(
            ok => mapper(ok),
            err => Result<TNew, TE>.Err(err));
    }


    public static Result<TNew, TE> Map<T, TNew, TE>(
        this Result<T, TE> result,
        Func<T, TNew> mapper)
    {
        return result.Match(
            ok => Result<TNew, TE>.Ok(mapper(ok)),
            err => Result<TNew, TE>.Err(err));
    }

    public static async Task<Result<TNew, TE>> MapToResultAsync<T, TNew, TE>(
        this Task<Result<T, TE>> resultTask,
        Func<T, TNew> mapper)
    {
        var result = await resultTask;
        return result.Match(
            ok => Result<TNew, TE>.Ok(mapper(ok)),
            err => Result<TNew, TE>.Err(err));
    }
        

    public static async Task<Result<TNew, TE>> BindAsync<T, TNew, TE>(
        this Task<Result<T, TE>> resultTask, 
        Func<T, Task<Result<TNew, TE>>> mapper)
    {
        var result = await resultTask;
        return await result.Match(
            async ok => await mapper(ok),
            err => Task.FromResult(Result<TNew, TE>.Err(err))
        );
    }
}



