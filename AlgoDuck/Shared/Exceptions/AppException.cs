namespace AlgoDuck.Shared.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public AppException(string message, int statusCode = 500) : base(message)
    {
        StatusCode = statusCode;
    }
    public AppException(string message, int statusCode, Exception innerException) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}