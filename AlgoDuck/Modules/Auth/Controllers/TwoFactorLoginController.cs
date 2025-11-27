using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Interfaces;
using AlgoDuck.Modules.Auth.Jwt;
using AlgoDuck.Modules.Auth.TwoFactor;
using AlgoDuck.Shared.Http;
using Status = AlgoDuck.Shared.Http.Status;

namespace AlgoDuck.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("AuthTight")]
    public sealed class TwoFactorLoginController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;
        private readonly ITwoFactorService _twofa;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _db;
        private readonly JwtSettings _jwt;
        private readonly IWebHostEnvironment _env;

        public TwoFactorLoginController(
            UserManager<ApplicationUser> userManager,
            IAuthService authService,
            ITwoFactorService twofa,
            ITokenService tokenService,
            ApplicationDbContext db,
            IOptions<JwtSettings> jwt,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _authService = authService;
            _twofa = twofa;
            _tokenService = tokenService;
            _db = db;
            _jwt = jwt.Value;
            _env = env;
        }

        public sealed class LoginStartRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public sealed class LoginVerifyRequest
        {
            public string ChallengeId { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
        }

        [HttpPost("login-start")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginStart([FromBody] LoginStartRequest dto, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null) return Unauthorized(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Invalid credentials"
            });

        var ok = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!ok) return Unauthorized(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Invalid credentials"
            });

            var twofaEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!twofaEnabled)
            {
                await _authService.LoginAsync(
                    new DTOs.LoginDto
                    {
                        Username = dto.Email,
                        Password = dto.Password
                    },
                    Response,
                    ct
                );

                return Ok(new StandardApiResponse
                {
                    Message = "Logged in succesfully"
                });
            }

            var (challengeId, expiresAt) = await _twofa.SendLoginCodeAsync(user, ct);
            var methods = new[] { "email" };
            return Ok(new StandardApiResponse<LoginResponseDto>
            {
                Body = new LoginResponseDto
                {
                    TwoFactoRequired = true,
                    ChallengeId = challengeId,
                    ExpiresAt = expiresAt,
                    Methods = methods
                }
            });
        }

        private class LoginResponseDto
        {
            public required bool TwoFactoRequired { get; set; }
            public required string ChallengeId { get; set; } = string.Empty;
            public required DateTimeOffset ExpiresAt { get; set; }
            public required string[] Methods { get; set; }
        }

        [HttpPost("login-verify")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginVerify([FromBody] LoginVerifyRequest dto, CancellationToken ct)
        {
            var (ok, userId, error) = await _twofa.VerifyLoginCodeAsync(dto.ChallengeId, dto.Code, ct);
            if (!ok) return BadRequest(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Invalid code"
            });

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) return Unauthorized(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Unauthorized"
            });

            var accessToken = await _tokenService.CreateAccessTokenAsync(user);

            var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var saltBytes = Shared.Utilities.HashingHelper.GenerateSalt();
            var hashB64 = Shared.Utilities.HashingHelper.HashPassword(rawRefresh, saltBytes);
            var saltB64 = Convert.ToBase64String(saltBytes);
            
            var prefixLength = Math.Min(rawRefresh.Length, 32);
            var refreshPrefix = rawRefresh.Substring(0, prefixLength);

            var session = new Session
            {
                SessionId = Guid.NewGuid(),
                RefreshTokenHash = hashB64,
                RefreshTokenSalt = saltB64,
                RefreshTokenPrefix = refreshPrefix,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwt.RefreshDays),
                UserId = user.Id,
                User = user
            };

            await _db.Sessions.AddAsync(session, ct);
            await _db.SaveChangesAsync(ct);

            SetJwtCookie(Response, accessToken, DateTimeOffset.UtcNow.AddMinutes(_jwt.DurationInMinutes));
            SetRefreshCookie(Response, rawRefresh, session.ExpiresAtUtc);
            SetCsrfCookie(Response);

            return Ok(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Logged in with 2fa"
            });
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