using AlgoDuck.Modules.Auth.Shared.Constants;

namespace AlgoDuck.Modules.Auth.Shared.Configuration;

public sealed class ApiKeyConfiguration
{
    public const string SectionName = "Auth:ApiKeys";

    public bool Enabled { get; init; } = true;
    public int KeyLength { get; init; } = ApiKeyConstants.KeyLength;
    public int MaxActiveKeysPerUser { get; init; } = ApiKeyConstants.MaxActiveKeysPerUser;
}