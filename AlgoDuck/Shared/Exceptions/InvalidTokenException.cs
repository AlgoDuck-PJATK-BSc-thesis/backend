namespace AlgoDuck.Shared.Exceptions;

public class InvalidTokenException : AppException
{
    public InvalidTokenException(string message = "Invalid or malformed token.") 
        : base(message, 401) { }

    public InvalidTokenException(string message, Exception innerException) 
        : base(message, 401, innerException) { }
}