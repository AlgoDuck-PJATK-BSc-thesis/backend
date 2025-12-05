namespace AlgoDuck.Modules.Auth.Shared.Constants;

public static class TokenConstants
{
    public const string AccessTokenAudience = "algoduck_api";
    public const string AccessTokenIssuer = "algoduck_auth";

    public const int AccessTokenLifetimeMinutes = 15;
    public const int RefreshTokenLifetimeDays = 30;

    public const string RefreshTokenType = "refresh";
    public const string AccessTokenType = "access";

    public const string ExpiredHeaderName = "X-Auth-Expired";
    public const string ExpiredHeaderValue = "true";
}