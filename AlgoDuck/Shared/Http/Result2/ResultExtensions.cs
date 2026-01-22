namespace AlgoDuck.Shared.Http.Result2;

public static  class ResultExtensions
{
       public static Result<TNew, TE> Bind<T, TNew, TE>(
        this Result<T, TE> result, 
        Func<T, Result<TNew, TE>> mapper) where TE : IToActionResultable
    {
        return result.Match(
            mapper,
            Result<TNew, TE>.Err
        );
    }
       
    public static async Task<Result<TNew, TE>> BindAsync<T, TNew, TE>(
        this Result<T, TE> result, 
        Func<T, Task<Result<TNew, TE>>> mapper)  where TE : IToActionResultable
    {
        return await result.Match(
            async ok => await mapper(ok),
            err => Task.FromResult(Result<TNew, TE>.Err(err))
        );
    }
    
    public static async Task<Result<TNew, TE>> BindResult<T, TNew, TE>(
        this Task<Result<T, TE>> resultTask, 
        Func<T, Result<TNew, TE>> mapper)  where TE : IToActionResultable
    {
        var result = await resultTask;
        return result.Match(
            ok => mapper(ok),
            err => Result<TNew, TE>.Err(err)
        );
    }
    
    

    public static async Task<Result<TNew, TE>> MapAsync<T, TNew, TE>(
        this Task<Result<T, TE>> resultTask,
        Func<T, Result<TNew, TE>> mapper)  where TE : IToActionResultable
    {
        var result = await resultTask;
        return result.Match(
            ok => mapper(ok),
            err => Result<TNew, TE>.Err(err));
    }


    public static Result<TNew, TE> Map<T, TNew, TE>(
        this Result<T, TE> result,
        Func<T, TNew> mapper)  where TE : IToActionResultable
    {
        return result.Match(
            ok => Result<TNew, TE>.Ok(mapper(ok)),
            err => Result<TNew, TE>.Err(err));
    }

    public static async Task<Result<TNew, TE>> MapToResultAsync<T, TNew, TE>(
        this Task<Result<T, TE>> resultTask,
        Func<T, TNew> mapper)  where TE : IToActionResultable
    {
        var result = await resultTask;
        return result.Match(
            ok => Result<TNew, TE>.Ok(mapper(ok)),
            err => Result<TNew, TE>.Err(err));
    }
        

    public static async Task<Result<TNew, TE>> BindAsync<T, TNew, TE>(
        this Task<Result<T, TE>> resultTask, 
        Func<T, Task<Result<TNew, TE>>> mapper)  where TE : IToActionResultable
    {
        var result = await resultTask;
        return await result.Match(
            async ok => await mapper(ok),
            err => Task.FromResult(Result<TNew, TE>.Err(err))
        );
    }
}