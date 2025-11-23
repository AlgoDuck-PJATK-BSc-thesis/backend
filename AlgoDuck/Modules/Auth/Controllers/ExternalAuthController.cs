using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Interfaces;
using AlgoDuck.Modules.Auth.Jwt;

namespace AlgoDuck.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public sealed class ExternalAuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _db;
        private readonly JwtSettings _jwt;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ExternalAuthController> _log;

        public ExternalAuthController(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            ApplicationDbContext db,
            IOptions<JwtSettings> jwt,
            IWebHostEnvironment env,
            ILogger<ExternalAuthController> log)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _db = db;
            _jwt = jwt.Value;
            _env = env;
            _log = log;
        }

        [HttpGet("oauth/{provider}/start")]
        public IActionResult Start([FromRoute] string provider, [FromQuery] string returnUrl = "/")
        {
            var scheme = Normalize(provider);
            var redirectUri = Url.Action(nameof(ExternalCallback), "ExternalAuth", new { provider, returnUrl }, Request.Scheme);
            var props = new AuthenticationProperties { RedirectUri = redirectUri };
            return Challenge(props, scheme);
        }

        [HttpGet("oauth/{provider}")]
        public async Task<IActionResult> ExternalCallback([FromRoute] string provider, [FromQuery] string returnUrl = "/")
        {
            var target = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";

            var auth = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (!auth.Succeeded || auth.Principal is null)
            {
                _log.LogWarning("External auth failed for provider={Provider}", provider);
                return LocalRedirect("/");
            }

            var scheme = Normalize(provider);
            var external = auth.Principal;
            var providerKey = external.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = external.FindFirstValue(ClaimTypes.Email) ?? external.FindFirstValue("email");
            var displayName =
                external.FindFirstValue(ClaimTypes.Name) ??
                external.FindFirstValue("name") ??
                external.FindFirstValue("urn:github:login") ??
                "user";

            ApplicationUser? user = null;

            if (!string.IsNullOrWhiteSpace(providerKey))
            {
                user = await _userManager.FindByLoginAsync(scheme, providerKey);
            }

            if (user is null && !string.IsNullOrWhiteSpace(email))
            {
                user = await _userManager.FindByEmailAsync(email);
            }

            if (user is null)
            {
                var usernameBase = displayName.Replace(" ", "").ToLowerInvariant();
                var username = await UniqueUsername(usernameBase);

                user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = !string.IsNullOrWhiteSpace(email),
                    LockoutEnabled = true,
                    CohortId = null,
                    Coins = 0,
                    Experience = 0,
                    AmountSolved = 0
                };

                var create = await _userManager.CreateAsync(user);
                if (!create.Succeeded)
                {
                    _log.LogWarning("User create failed: {Reason}", string.Join("; ", create.Errors.Select(e => e.Description)));
                    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                    return LocalRedirect("/");
                }

                if (!await _userManager.IsInRoleAsync(user, "user"))
                {
                    await _userManager.AddToRoleAsync(user, "user");
                }
            }

            if (!string.IsNullOrWhiteSpace(providerKey))
            {
                var logins = await _userManager.GetLoginsAsync(user);
                if (!logins.Any(l => l.LoginProvider == scheme))
                {
                    var addLogin = await _userManager.AddLoginAsync(user, new UserLoginInfo(scheme, providerKey, scheme));
                    if (!addLogin.Succeeded)
                    {
                        _log.LogWarning("AddLogin failed: {Reason}", string.Join("; ", addLogin.Errors.Select(e => e.Description)));
                    }
                }
            }

            var accessToken = await _tokenService.CreateAccessTokenAsync(user);

            var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var saltBytes = Shared.Utilities.HashingHelper.GenerateSalt();
            var hashB64 = Shared.Utilities.HashingHelper.HashPassword(rawRefresh, saltBytes);
            var saltB64 = Convert.ToBase64String(saltBytes);

            var session = new Session
            {
                SessionId = Guid.NewGuid(),
                RefreshTokenHash = hashB64,
                RefreshTokenSalt = saltB64,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwt.RefreshDays),
                UserId = user.Id,
                User = user
            };

            await _db.Sessions.AddAsync(session);
            await _db.SaveChangesAsync();

            SetJwtCookie(Response, accessToken, DateTimeOffset.UtcNow.AddMinutes(_jwt.DurationInMinutes));
            SetRefreshCookie(Response, rawRefresh, session.ExpiresAtUtc);
            SetCsrfCookie(Response);

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            _log.LogInformation("External auth success provider={Provider} user={UserId}", scheme, user.Id);

            return LocalRedirect(target);
        }

        private async Task<string> UniqueUsername(string baseName)
        {
            var name = string.IsNullOrWhiteSpace(baseName) ? "user" : baseName;
            var candidate = name;
            var i = 0;
            while (await _userManager.FindByNameAsync(candidate) is not null)
            {
                i++;
                candidate = $"{name}{i}";
            }
            return candidate;
        }

        private static string Normalize(string provider)
        {
            var p = provider.ToLowerInvariant();
            return p switch
            {
                "google" => "Google",
                "github" => "GitHub",
                "facebook" => "Facebook",
                _ => provider
            };
        }

        private void SetJwtCookie(HttpResponse response, string accessToken, DateTimeOffset expires)
        {
            var domain = string.IsNullOrWhiteSpace(_jwt.CookieDomain) ? null : _jwt.CookieDomain;
            response.Cookies.Append(_jwt.JwtCookieName, accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                Expires = expires,
                Path = "/",
                Domain = domain
            });
        }

        private void SetRefreshCookie(HttpResponse response, string rawRefresh, DateTime expiresUtc)
        {
            var domain = string.IsNullOrWhiteSpace(_jwt.CookieDomain) ? null : _jwt.CookieDomain;
            response.Cookies.Append(_jwt.RefreshCookieName, rawRefresh, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                Expires = new DateTimeOffset(expiresUtc),
                Path = "/api/auth/refresh",
                Domain = domain
            });
        }

        private void SetCsrfCookie(HttpResponse response)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            var domain = string.IsNullOrWhiteSpace(_jwt.CookieDomain) ? null : _jwt.CookieDomain;
            response.Cookies.Append(_jwt.CsrfCookieName, token, new CookieOptions
            {
                HttpOnly = false,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                Path = "/",
                Domain = domain
            });
            response.Headers[_jwt.CsrfHeaderName] = token;
        }
    }
}