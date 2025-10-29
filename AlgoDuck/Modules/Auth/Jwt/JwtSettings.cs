namespace AlgoDuck.Modules.Auth.Jwt;

public class JwtSettings
{
    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public double DurationInMinutes { get; set; } = 60;
    
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public int ClockSkewSeconds { get; set; } = 60;

    public int RefreshDays { get; set; } = 30;
    public string RefreshCookieName { get; set; } = "refresh_token";
    public string JwtCookieName { get; set; } = "jwt";
    public string CsrfCookieName { get; set; } = "csrf_token";
    public string CsrfHeaderName { get; set; } = "X-CSRF-Token";
    public string? CookieDomain { get; set; }
    
}
