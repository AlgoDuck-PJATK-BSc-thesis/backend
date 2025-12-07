namespace AlgoDuck.Modules.User.Shared.Exceptions;

public class UserNotFoundException : Exception
{
    public Guid UserId { get; }

    public UserNotFoundException()
    {
    }

    public UserNotFoundException(Guid userId) : base($"User with id '{userId}' was not found.")
    {
        UserId = userId;
    }

    public UserNotFoundException(string message) : base(message)
    {
    }

    public UserNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}