namespace AlgoDuck.Modules.Auth.Shared.Jwt;

public sealed class JwtSettings
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;

    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenMinutes { get; init; } = 60 * 24 * 7;

    public string AccessTokenCookieName { get; init; } = "jwt";
    public string RefreshTokenCookieName { get; init; } = "refresh_token";
    public string CsrfCookieName { get; init; } = "csrf_token";
    public string CsrfHeaderName { get; init; } = "X-CSRF-Token";
    public string? CookieDomain { get; set; }
    
}