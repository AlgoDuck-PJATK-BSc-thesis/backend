namespace WebApplication1.Shared.Exceptions;

public class DuplicateResourceException : AppException
{
    public DuplicateResourceException(string message = "The resource already exists.")
        : base(message, 409) { }
}