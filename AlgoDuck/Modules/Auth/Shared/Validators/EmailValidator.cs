using System.Text.RegularExpressions;

namespace AlgoDuck.Modules.Auth.Shared.Validators;

public sealed class EmailValidator : BaseValidator
{
    private static readonly Regex EmailRegex = new Regex(
        "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public void Validate(string email)
    {
        EnsureNotNullOrWhiteSpace(email, "Email");
        Ensure(EmailRegex.IsMatch(email), "Email format is invalid.");
    }
}