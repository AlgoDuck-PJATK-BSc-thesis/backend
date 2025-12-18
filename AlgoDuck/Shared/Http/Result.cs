using OneOf;

namespace AlgoDuck.Shared.Http;

public class Result<T, TE> : OneOfBase<T, TE>
{
    private Result(OneOf<T, TE> input) : base(input) { }
    
    public static Result<T, TE> Ok(T value) => new(value);
    public static Result<T, TE> Err(TE error) => new(error);
    
    public bool IsOk => IsT0;
    public bool IsErr => IsT1;
}
