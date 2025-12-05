namespace AlgoDuck.Modules.Auth.Shared.Configuration;

public sealed class EnvironmentAuthConfiguration
{
    public const string SectionName = "Auth:Environment";

    public string JwtSigningKey { get; init; } = string.Empty;
    public string RefreshTokenSigningKey { get; init; } = string.Empty;
}