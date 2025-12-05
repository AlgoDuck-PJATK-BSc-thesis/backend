namespace AlgoDuck.Modules.Auth.Shared.Constants;

public static class ApiKeyConstants
{
    public const int KeyLength = 64;
    public const int NameMaxLength = 128;
    public const int DescriptionMaxLength = 512;

    public const string HeaderName = "X-Api-Key";

    public const int MaxActiveKeysPerUser = 10;
}