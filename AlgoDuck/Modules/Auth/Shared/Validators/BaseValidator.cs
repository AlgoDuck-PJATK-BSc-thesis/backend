using AlgoDuck.Modules.Auth.Shared.Exceptions;

namespace AlgoDuck.Modules.Auth.Shared.Validators;

public abstract class BaseValidator
{
    protected static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new ValidationException(message);
        }
    }

    protected static void EnsureNotNullOrWhiteSpace(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{fieldName} is required.");
        }
    }

    protected static void EnsureMaxLength(string value, int maxLength, string fieldName)
    {
        if (value.Length > maxLength)
        {
            throw new ValidationException($"{fieldName} must be at most {maxLength} characters long.");
        }
    }
}