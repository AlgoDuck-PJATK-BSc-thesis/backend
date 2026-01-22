using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Shared.Http.Result2;

public interface IToActionResultable
{
    public IActionResult ToActionResult(string? message = null);
}

public abstract class ResultError<TBody> : IToActionResultable
{
    public TBody Body { get; }
    protected abstract int StatusCode { get; }
    
    protected ResultError(TBody body) => Body = body;
    
    public IActionResult ToActionResult(string? message = null) =>
        new ObjectResult(new StandardApiResponse<TBody>
        {
            Status = Status.Error,
            Message = message ?? "",
            Body = Body
        }) { StatusCode = StatusCode };
}

public class NotFoundError<T>(T body) : ResultError<T>(body) { protected override int StatusCode => 404; }
public class BadRequestError<T>(T body) : ResultError<T>(body) { protected override int StatusCode => 400; }
public class UnauthorizedError<T>(T body) : ResultError<T>(body) { protected override int StatusCode => 401; }
public class ForbiddenError<T>(T body) : ResultError<T>(body) { protected override int StatusCode => 403; }
public class ConflictError<T>(T body) : ResultError<T>(body) { protected override int StatusCode => 409; }
public class ValidationError<T>(T body) : ResultError<T>(body) { protected override int StatusCode => 422; }
public class InternalError<T>(T body) : ResultError<T>(body) { protected override int StatusCode => 500; }