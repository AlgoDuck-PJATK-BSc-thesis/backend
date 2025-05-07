namespace WebApplication1.Shared.Exceptions;

public class ResourceGoneException : AppException
{
    public ResourceGoneException(string message = "The resource requested is no longer available.")
        : base(message, 410) { }
}