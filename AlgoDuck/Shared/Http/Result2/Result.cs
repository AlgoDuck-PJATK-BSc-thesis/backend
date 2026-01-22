using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace AlgoDuck.Shared.Http.Result2;


public class Result<T, TE> : OneOfBase<T, TE> 
    where TE : IToActionResultable
{
    private Result(OneOf<T, TE> input) : base(input) { }

    public static Result<T, TE> Ok(T value) => new(value);
    public static Result<T, TE> Err(TE error) => new(error);
    
    public bool IsOk => IsT0;
    public bool IsErr => IsT1;
    
    public T? AsOk => IsT0 ? AsT0 : default;
    public TE? AsErr => IsT1 ? AsT1 : default;
    
    public IActionResult ToActionResult(string? message = null) =>
        Match(
            ok => new OkObjectResult(new StandardApiResponse<T>
            {
                Status = Status.Success,
                Message = message ?? "",
                Body = ok
            }),
            err => err.ToActionResult(message)
        );
        
}

public static class HttpExtensions
{
    public static async Task<IActionResult> ToActionResultAsync<T, TE>(
        this Task<Result<T, TE>> resultTask, string message = "") where TE : IToActionResultable
    {
        var result = await resultTask;
        return result.ToActionResult(message);
    }
}