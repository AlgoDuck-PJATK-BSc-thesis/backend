using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace AlgoDuck.Shared.Result;

public readonly struct Result<T, TE> where TE : IResultError
{
    private readonly OneOf<T, TE> _inner;

    private Result(OneOf<T, TE> input)
    {
        _inner = input;
    }

    public static Result<T, TE> Ok(T value) => new(value);
    public static Result<T, TE> Err(TE error) => new(error);

    public bool IsOk => _inner.IsT0;
    public bool IsT0 => _inner.IsT0;
    public bool IsErr => _inner.IsT1;
    public bool IsT1 => _inner.IsT1;
    
    public T? AsOk => IsOk ? _inner.AsT0 : default;
    public T AsT0 => _inner.AsT0;
    public TE? AsErr => IsErr ? _inner.AsT1 : default;
    public TE AsT1 => _inner.AsT1;
    

    public TResult Match<TResult>(Func<T, TResult> onOk, Func<TE, TResult> onErr)
        => _inner.Match(onOk, onErr);

    public async Task<TResult> Match<TResult>(Func<T, Task<TResult>> onOk, Func<TE, Task<TResult>> onErr)
        => await _inner.Match(onOk, onErr);

    public void Switch(Action<T> onOk, Action<TE> onErr)
        => _inner.Switch(onOk, onErr);
}

public static class Result2Extensions
{
    public static async Task<IActionResult> ToActionResultAsync<T, TE>(this Task<Result<T, TE>> resultTask, string message = "")
        where TE : IResultError
    {
        var result = await resultTask;
        return result.ToActionResult(message);
    }
    
    public static IActionResult ToActionResult<T, TE>(this Result<T, TE> result, string message  = "") where TE : IResultError
    {
        if (result.IsOk)
            return new OkObjectResult(new StandardApiResponse<T>()
            {
                Message = message,
                Body = result.AsOk!
            });
        return result.AsErr!.ToActionResult(message);
    }
}