namespace AlgoDuck.Shared.Exceptions;

public class TokenExpiredException : AppException
{
    public TokenExpiredException(string message = "The token has expired.")
        : base(message, 401) { }
}