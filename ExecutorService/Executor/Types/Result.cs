using OneOf;

namespace ExecutorService.Executor.Types;

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