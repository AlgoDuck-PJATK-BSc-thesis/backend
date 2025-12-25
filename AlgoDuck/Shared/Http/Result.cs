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

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T, TE>(
        this Result<T, ErrorObject<TE>> result)
    {
        return result.Match(
            ok => new OkObjectResult(new StandardApiResponse<T>
            {
                Body = ok
            }),
            err => err.Type switch
            {
                ErrorType.BadRequest or ErrorType.ValidationError => new BadRequestObjectResult(new StandardApiResponse<TE>
                {
                    Status = Status.Error,
                    Body = err.Body
                }),
                ErrorType.NotFound => new NotFoundObjectResult(new StandardApiResponse<TE>
                {
                    Status = Status.Error,
                    Body = err.Body
                }),
                ErrorType.Unauthorized => new UnauthorizedObjectResult(new StandardApiResponse<TE>
                {
                    Status = Status.Error,
                    Body = err.Body
                }),
                ErrorType.Forbidden => new ObjectResult(new StandardApiResponse<TE>
                {
                    Status = Status.Error,
                    Body = err.Body
                }) { StatusCode = 403 },
                ErrorType.Conflict => new ConflictObjectResult(new StandardApiResponse<TE>
                {
                    Status = Status.Error,
                    Body = err.Body
                }),
                _ => new ObjectResult(new StandardApiResponse<string>
                {
                    Status = Status.Error,
                    Body = "An unexpected error occurred"
                }) { StatusCode = 500 }
            }
        );
    }
}
