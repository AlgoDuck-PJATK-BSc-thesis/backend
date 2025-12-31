using System.Security.Claims;
using AlgoDuck.Modules.Auth.Commands.ExternalLogin;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using AlgoDuck.Modules.Auth.Shared.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Auth.Commands.OAuthLogin;

[ApiController]
[Route("api/auth/oauth")]
public sealed class OAuthLoginEndpoint : ControllerBase
{
    private readonly IExternalLoginHandler _externalLoginHandler;
    private readonly JwtSettings _jwtSettings;
    private readonly IConfiguration _configuration;

    public OAuthLoginEndpoint(
        IExternalLoginHandler externalLoginHandler,
        IOptions<JwtSettings> jwtOptions,
        IConfiguration configuration)
    {
        _externalLoginHandler = externalLoginHandler;
        _jwtSettings = jwtOptions.Value;
        _configuration = configuration;
    }

    [HttpGet("{provider}/start")]
    [AllowAnonymous]
    public IActionResult Start(
        [FromRoute] string provider,
        [FromQuery] string? returnUrl = null,
        [FromQuery] string? errorUrl = null,
        [FromQuery] string? prompt = null)
    {
        var scheme = ToScheme(provider);
        if (scheme is null)
        {
            return BadRequest("Unsupported OAuth provider.");
        }

        var providerKey = NormalizeProviderKey(provider);
        var safeReturnUrl = SafeRelative(returnUrl, "/home");
        var safeErrorUrl = SafeRelative(errorUrl, "/login");

        var redirectUri =
            $"/api/auth/oauth/{providerKey}/complete?returnUrl={Uri.EscapeDataString(safeReturnUrl)}&errorUrl={Uri.EscapeDataString(safeErrorUrl)}";

        var props = new AuthenticationProperties
        {
            RedirectUri = redirectUri
        };

        if (providerKey == "microsoft")
        {
            var p = (prompt ?? string.Empty).Trim();
            if (p == "select_account" || p == "login")
            {
                props.Items["prompt"] = p;
            }
        }

        return Challenge(props, scheme);
    }

    [HttpGet("{provider}/complete")]
    [AllowAnonymous]
    public async Task<IActionResult> Complete(
        [FromRoute] string provider,
        [FromQuery] string? returnUrl = null,
        [FromQuery] string? errorUrl = null,
        CancellationToken cancellationToken = default)
    {
        var scheme = ToScheme(provider);
        if (scheme is null)
        {
            return Redirect(BuildFrontendRedirect(errorUrl, provider, "unsupported_provider"));
        }

        var providerKey = NormalizeProviderKey(provider);
        var safeReturnUrl = SafeRelative(returnUrl, "/home");

        var oauthError = (Request.Query["error"].ToString() ?? string.Empty).Trim();
        if (oauthError.Equals("access_denied", StringComparison.OrdinalIgnoreCase))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return Redirect(BuildFrontendRedirect(errorUrl, providerKey, "access_denied"));
        }

        var externalAuth = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        if (!externalAuth.Succeeded || externalAuth.Principal is null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return Redirect(BuildFrontendRedirect(errorUrl, providerKey, "oauth_failed"));
        }

        var principal = externalAuth.Principal;

        var externalUserId =
            principal.FindFirstValue("oid") ??
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
            principal.FindFirstValue("sub") ??
            string.Empty;

        var email =
            principal.FindFirstValue(ClaimTypes.Email) ??
            principal.FindFirstValue("email") ??
            principal.FindFirstValue("preferred_username") ??
            principal.FindFirstValue("upn") ??
            string.Empty;

        var displayName =
            principal.FindFirstValue(ClaimTypes.Name) ??
            principal.FindFirstValue("name") ??
            principal.FindFirstValue("preferred_username") ??
            principal.FindFirstValue("urn:github:login") ??
            email;

        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return Redirect(BuildFrontendRedirect(errorUrl, providerKey, "missing_user_id"));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return Redirect(BuildFrontendRedirect(errorUrl, providerKey, "missing_email"));
        }

        try
        {
            var dto = new ExternalLoginDto
            {
                Provider = providerKey,
                ExternalUserId = externalUserId,
                Email = email,
                DisplayName = displayName
            };

            var authResponse = await _externalLoginHandler.HandleAsync(dto, cancellationToken);

            AuthCookieWriter.SetAuthCookies(Response, _jwtSettings, authResponse);

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return Redirect(BuildFrontendSuccessRedirect(safeReturnUrl));
        }
        catch
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return Redirect(BuildFrontendRedirect(errorUrl, providerKey, "login_failed"));
        }
    }

    private string BuildFrontendSuccessRedirect(string returnUrl)
    {
        var baseUrl = GetFrontendBaseUrl();
        return $"{baseUrl}{returnUrl}";
    }

    private string BuildFrontendRedirect(string? errorUrl, string providerKey, string reason)
    {
        var baseUrl = GetFrontendBaseUrl();
        var safeErrorUrl = SafeRelative(errorUrl, "/login");

        var separator = safeErrorUrl.Contains('?') ? "&" : "?";
        return $"{baseUrl}{safeErrorUrl}{separator}oauthError={Uri.EscapeDataString(reason)}&provider={Uri.EscapeDataString(providerKey)}";
    }

    private string GetFrontendBaseUrl()
    {
        var url = _configuration["App:FrontendUrl"];
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException("Missing App:FrontendUrl (set APP__FRONTENDURL).");
        }
        return url.Trim().TrimEnd('/');
    }

    private static string SafeRelative(string? url, string fallback)
    {
        if (string.IsNullOrWhiteSpace(url)) return fallback;
        var trimmed = url.Trim();
        if (!trimmed.StartsWith('/')) return fallback;
        if (trimmed.StartsWith("//")) return fallback;
        return trimmed;
    }

    private static string NormalizeProviderKey(string provider)
    {
        return provider.Trim().ToLowerInvariant();
    }

    private static string? ToScheme(string provider)
    {
        var p = provider.Trim().ToLowerInvariant();
        if (p is "google") return "Google";
        if (p is "github") return "GitHub";
        if (p is "facebook") return "Facebook";
        if (p is "microsoft") return "Microsoft";
        return null;
    }
}
