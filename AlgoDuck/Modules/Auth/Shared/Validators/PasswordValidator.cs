namespace AlgoDuck.Modules.Auth.Shared.Validators;

public sealed class PasswordValidator : BaseValidator
{
    private const int MinimumLength = 8;
    private const int MaximumLength = 128;

    public void Validate(string password)
    {
        EnsureNotNullOrWhiteSpace(password, "Password");
        Ensure(password.Length >= MinimumLength, $"Password must be at least {MinimumLength} characters long.");
        Ensure(password.Length <= MaximumLength, $"Password must be at most {MaximumLength} characters long.");
        Ensure(HasUpper(password), "Password must contain at least one uppercase letter.");
        Ensure(HasLower(password), "Password must contain at least one lowercase letter.");
        Ensure(HasDigit(password), "Password must contain at least one digit.");
    }

    private static bool HasUpper(string value)
    {
        foreach (var c in value)
        {
            if (char.IsUpper(c))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasLower(string value)
    {
        foreach (var c in value)
        {
            if (char.IsLower(c))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasDigit(string value)
    {
        foreach (var c in value)
        {
            if (char.IsDigit(c))
            {
                return true;
            }
        }

        return false;
    }
}