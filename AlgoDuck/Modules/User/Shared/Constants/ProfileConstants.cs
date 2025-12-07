namespace AlgoDuck.Modules.User.Shared.Constants;

public static class ProfileConstants
{
    public const int MaxUsernameLength = 32;
    public const int MinUsernameLength = 3;
    public const int MaxDisplayNameLength = 64;

    public const string DefaultLanguage = "en";
    public const string PolishLanguage = "pl";

    public static readonly string[] AllowedLanguages =
    {
        DefaultLanguage,
        PolishLanguage
    };
}