namespace AlgoDuck.Modules.Auth.Shared.Jwt;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenMinutes { get; set; } = 60 * 24 * 7;

    public string AccessTokenCookieName { get; set; } = "jwt";
    public string RefreshTokenCookieName { get; set; } = "refresh_token";
    public string CsrfCookieName { get; set; } = "csrf_token";
    public string CsrfHeaderName { get; set; } = "X-CSRF-Token";
    public string? CookieDomain { get; set; }

    public int DurationInMinutes
    {
        get => AccessTokenMinutes;
        set => AccessTokenMinutes = value;
    }

    public int RefreshDays
    {
        get => RefreshTokenMinutes / (60 * 24);
        set => RefreshTokenMinutes = checked(value * 60 * 24);
    }

    public string JwtCookieName
    {
        get => AccessTokenCookieName;
        set => AccessTokenCookieName = value;
    }

    public string RefreshCookieName
    {
        get => RefreshTokenCookieName;
        set => RefreshTokenCookieName = value;
    }

    public string? Key
    {
        get => SigningKey;
        set => SigningKey = value ?? string.Empty;
    }
}