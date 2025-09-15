namespace AlgoDuck.Shared.Exceptions;

public class DatabaseException : AppException
{
    public DatabaseException(string message = "Database operation failed")
        : base(message, 500) { }
}