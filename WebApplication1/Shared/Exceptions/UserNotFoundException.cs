namespace WebApplication1.Shared.Exceptions;

public class UserNotFoundException : AppException
{
    public UserNotFoundException(string message = "User not found.")
        : base(message, 404) { }
}