namespace AlgoDuck.Modules.Cohort.Shared.Exceptions;

public sealed class ChatValidationException : CohortException
{
    public string? Category { get; }

    public ChatValidationException(string message)
        : base("chat_validation_error", message)
    {
    }

    public ChatValidationException(string message, string? category)
        : base("chat_validation_error", message)
    {
        Category = category;
    }

    public ChatValidationException(string message, string? category, Exception? innerException)
        : base("chat_validation_error", message, innerException)
    {
        Category = category;
    }
}