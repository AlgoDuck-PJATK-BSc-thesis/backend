namespace AlgoDuck.Modules.User.Shared.Exceptions;

public class ProfileException : Exception
{
    public ProfileException()
    {
    }

    public ProfileException(string message) : base(message)
    {
    }

    public ProfileException(string message, Exception innerException) : base(message, innerException)
    {
    }
}