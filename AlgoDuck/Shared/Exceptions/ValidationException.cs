namespace AlgoDuck.Shared.Exceptions;

public class ValidationException : AppException
{
    public ValidationException(string message = "One or more validation errors occurred.")
        : base(message, 422) { }
}