namespace AlgoDuck.Modules.Cohort.Shared.Exceptions;

public sealed class ChatValidationException : CohortException
{
    public string? Category { get; }

    public ChatValidationException(string message, string? category)
        : base("chat_validation_error", message)
    {
        Category = category;
    }

    public ChatValidationException(string message, int statusCode = 400)
        : base("chat_validation_error", message, statusCode)
    {
    }

    public ChatValidationException(string message, int statusCode, Exception? innerException)
        : base("chat_validation_error", message, statusCode, innerException)
    {
    }

    public ChatValidationException(string message, string? category, int statusCode)
        : base("chat_validation_error", message, statusCode)
    {
        Category = category;
    }
}