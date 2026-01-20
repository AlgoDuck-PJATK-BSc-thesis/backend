namespace AlgoDuck.Modules.Auth.Shared.Validators;

public sealed class ApiKeyValidator : BaseValidator
{
    private const int NameMaxLength = 256;

    public void ValidateName(string name)
    {
        EnsureNotNullOrWhiteSpace(name, "API key name");
        EnsureMaxLength(name, NameMaxLength, "API key name");
    }
}