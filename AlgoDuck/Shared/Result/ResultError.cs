using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Shared.Result;

public interface IToActionResultable
{
    public IActionResult ToActionResult(string? message = null);
}

public interface IResultError : IToActionResultable;

public interface IResultError<TSelf, TBody> : IResultError
    where TSelf : struct, IResultError<TSelf, TBody>
{
    static abstract int StatusCode { get; }
    TBody Body { get; }
    IActionResult IToActionResultable.ToActionResult(string? message)
    {
        return new ObjectResult(new StandardApiResponse<TBody>
        {
            Status = Status.Error,
            Message = message ?? "",
            Body = Body
        })
        {
            StatusCode = TSelf.StatusCode
        };
    }
}

public readonly struct NotFoundError<TBody>(TBody body) : IResultError<NotFoundError<TBody>, TBody>
{
    public static int StatusCode => 404;
    public TBody Body => body;
}

public readonly struct UserNotFoundError(string body) : IResultError<UserNotFoundError, string>
{
    public static int StatusCode => 404;
    public string Body => body;
}

public readonly struct BadRequestError<TBody>(TBody body) : IResultError<BadRequestError<TBody>, TBody>
{
    public static int StatusCode => 400;
    public TBody Body => body;
}

public readonly struct UnauthorizedError<TBody>(TBody body) : IResultError<UnauthorizedError<TBody>, TBody>
{
    public static int StatusCode => 401;
    public TBody Body => body;
}

public readonly struct ForbiddenError<TBody>(TBody body) : IResultError<ForbiddenError<TBody>, TBody>
{
    public static int StatusCode => 403;
    public TBody Body => body;
}

public readonly struct ConflictError<TBody>(TBody body) : IResultError<ConflictError<TBody>, TBody>
{
    public static int StatusCode => 409;
    public TBody Body => body;
}

public readonly struct ValidationError<TBody>(TBody body) : IResultError<ValidationError<TBody>, TBody>
{
    public static int StatusCode => 422;
    public TBody Body => body;
}

public readonly struct InternalError<TBody>(TBody body) : IResultError<InternalError<TBody>, TBody>
{
    public static int StatusCode => 500;
    public TBody Body => body;
}