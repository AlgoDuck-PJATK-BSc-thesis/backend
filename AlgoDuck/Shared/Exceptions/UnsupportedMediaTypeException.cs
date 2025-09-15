namespace AlgoDuck.Shared.Exceptions;

public class UnsupportedMediaTypeException : AppException
{
    public UnsupportedMediaTypeException(string message = "The media type is not supported.")
        : base(message, 415) { }
}