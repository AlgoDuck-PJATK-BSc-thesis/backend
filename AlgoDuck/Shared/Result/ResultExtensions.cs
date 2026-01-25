using FluentValidation;

namespace AlgoDuck.Shared.Result;

public static partial class ResultExtensions
{
    // TODO: this should probs have a MapAsyncWithAsync and MapWithAsync funcs
    public static async Task<Result<TNew, TE>> MapAsync<T, TNew, TE>(
        this Task<Result<T, TE>> resultTask, Func<T, TNew> mapper)
        where TE : struct, IResultError
    {
        var result = await resultTask;
        return result.Match(
            ok => Result<TNew, TE>.Ok(mapper(ok)),
            Result<TNew, TE>.Err);
    }
    
    public static Result<TNew, TE> Map<T, TNew, TE>(
        this Result<T, TE> result, Func<T, TNew> mapper)
        where TE : struct, IResultError
    {
        return result.Match(
            ok => Result<TNew, TE>.Ok(mapper(ok)),
            Result<TNew, TE>.Err);
    }
    
    public static async Task<Result<TNew, TE>> Map<T, TNew, TE>(
        this Task<Result<T, TE>> resultTask, Func<T, TNew> mapper)
        where TE : struct, IResultError
    {
        var result = await resultTask;
        return result.Match(
            ok => Result<TNew, TE>.Ok(mapper(ok)),
            Result<TNew, TE>.Err);
    }
}