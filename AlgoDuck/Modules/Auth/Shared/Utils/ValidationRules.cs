using System.Text.RegularExpressions;

namespace AlgoDuck.Modules.Auth.Shared.Utils;

public static class ValidationRules
{
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 128;
    public const int UserNameMaxLength = 64;
    public const int EmailMaxLength = 256;

    public static readonly Regex EmailRegex = new Regex(
        "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
}