using System;
using AlgoDuck.Modules.Auth.Shared.Constants;

namespace AlgoDuck.Modules.Auth.Shared.Configuration;

public sealed class AuthConfiguration
{
    public const string SectionName = "Auth";

    public string AccessTokenIssuer { get; init; } = TokenConstants.AccessTokenIssuer;
    public string AccessTokenAudience { get; init; } = TokenConstants.AccessTokenAudience;

    public int AccessTokenLifetimeMinutes { get; init; } = TokenConstants.AccessTokenLifetimeMinutes;
    public int RefreshTokenLifetimeDays { get; init; } = TokenConstants.RefreshTokenLifetimeDays;

    public string RefreshTokenCookieName { get; init; } = AuthConstants.RefreshTokenCookieName;
    public string CsrfTokenCookieName { get; init; } = AuthConstants.CsrfTokenCookieName;
    public string CsrfTokenHeaderName { get; init; } = AuthConstants.CsrfTokenHeaderName;

    public bool RequireHttpsCookies { get; init; } = true;
    public bool SlidingRefreshExpiration { get; init; } = true;

    public TimeSpan AccessTokenLifetime => TimeSpan.FromMinutes(AccessTokenLifetimeMinutes);
    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(RefreshTokenLifetimeDays);
}