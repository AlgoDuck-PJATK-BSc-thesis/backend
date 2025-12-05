namespace AlgoDuck.Modules.Auth.Shared.Constants;

public static class ValidationConstants
{
    public const int EmailMaxLength = 256;
    public const int UserNameMinLength = 3;
    public const int UserNameMaxLength = 32;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 128;

    public const string PasswordRequiresDigit = "Password must contain at least one digit.";
    public const string PasswordRequiresUpper = "Password must contain at least one uppercase letter.";
    public const string PasswordRequiresLower = "Password must contain at least one lowercase letter.";
    public const string PasswordRequiresNonAlphanumeric = "Password must contain at least one non-alphanumeric character.";
}