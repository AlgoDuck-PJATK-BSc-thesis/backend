namespace AlgoDuck.Shared.Exceptions;

public class ServiceUnavailableException : AppException
{
    public ServiceUnavailableException(string message = "The service is currently unavailable. Please try again later.")
        : base(message, 503) { }
}