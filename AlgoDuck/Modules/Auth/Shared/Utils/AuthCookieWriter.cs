using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Jwt;

namespace AlgoDuck.Modules.Auth.Shared.Utils;

public static class AuthCookieWriter
{
    public static void SetAuthCookies(HttpResponse response, JwtSettings jwtSettings, AuthResponse authResponse)
    {
        var cookieDomain = jwtSettings.CookieDomain;
        var expires = authResponse.RefreshTokenExpiresAt.UtcDateTime;
        var secure = !string.IsNullOrWhiteSpace(cookieDomain);

        var jwtCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain,
            Expires = expires
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain,
            Expires = expires
        };

        var csrfCookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain,
            Expires = expires
        };

        response.Cookies.Append(jwtSettings.AccessTokenCookieName, authResponse.AccessToken, jwtCookieOptions);
        response.Cookies.Append(jwtSettings.RefreshTokenCookieName, authResponse.RefreshToken, refreshCookieOptions);
        response.Cookies.Append(jwtSettings.CsrfCookieName, authResponse.CsrfToken, csrfCookieOptions);
    }
}