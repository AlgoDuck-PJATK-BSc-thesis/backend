using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AlgoDuck.DAL;
using AlgoDuck.Models.User;
using AlgoDuck.Modules.Auth.DTOs;
using AlgoDuck.Modules.Auth.Interfaces;
using AlgoDuck.Modules.Auth.Jwt;
using AlgoDuck.Models.Auth;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Utilities;

namespace AlgoDuck.Modules.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _dbContext;
        private readonly JwtSettings _jwt;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _http;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            ApplicationDbContext dbContext,
            IOptions<JwtSettings> options,
            IWebHostEnvironment env,
            IHttpContextAccessor http)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _dbContext = dbContext;
            _jwt = options.Value;
            _env = env;
            _http = http;
        }

        public async Task RegisterAsync(RegisterDto dto, CancellationToken cancellationToken)
        {
            if (await _userManager.FindByNameAsync(dto.Username) != null)
                throw new UsernameAlreadyExistsException();

            if (await _userManager.FindByEmailAsync(dto.Email) is not null)
                throw new EmailAlreadyExistsException();

            var defaultRole = await _dbContext.UserRoles
                .FirstOrDefaultAsync(r => r.Name == "user", cancellationToken);

            if (defaultRole == null)
                throw new NotFoundException("Default role 'user' not found.");

            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                CohortId = null,
                Coins = 0,
                Experience = 0,
                AmountSolved = 0,
                UserRoleId = defaultRole.UserRoleId
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new ValidationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        public async Task<(string AccessToken, string RefreshToken)> LoginAsync(LoginDto dto, HttpResponse response, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.UserName == dto.Username, cancellationToken);

            if (user == null)
                throw new UserNotFoundException();

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedException("Invalid password.");

            var accessToken = _tokenService.CreateAccessToken(user);

            var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var saltBytes = HashingHelper.GenerateSalt();
            var hashB64 = HashingHelper.HashPassword(rawRefresh, saltBytes);
            var saltB64 = Convert.ToBase64String(saltBytes);

            var session = new Session
            {
                RefreshTokenHash = hashB64,
                RefreshTokenSalt = saltB64,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwt.RefreshDays),
                UserId = user.Id,
                User = user
            };

            await _dbContext.Sessions.AddAsync(session, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            SetJwtCookie(response, accessToken, DateTimeOffset.UtcNow.AddMinutes(_jwt.DurationInMinutes));
            SetRefreshCookie(response, rawRefresh, session.ExpiresAtUtc);
            SetCsrfCookie(response);

            return (accessToken, rawRefresh);
        }

        public async Task RefreshTokenAsync(RefreshDto dto, HttpResponse response, CancellationToken cancellationToken)
        {
            var ctx = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context");
            var cookies = ctx.Request.Cookies;

            if (!cookies.TryGetValue(_jwt.RefreshCookieName, out var rawRefresh))
                throw new UnauthorizedException("Missing refresh token");

            var header = ctx.Request.Headers[_jwt.CsrfHeaderName].ToString();
            if (!cookies.TryGetValue(_jwt.CsrfCookieName, out var csrfCookie) || csrfCookie != header)
                throw new ForbiddenException("CSRF validation failed");

            var candidates = await _dbContext.Sessions
                .Include(s => s.User).ThenInclude(u => u.UserRole)
                .Where(s => s.RevokedAtUtc == null && s.ExpiresAtUtc > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            Session? session = null;
            foreach (var s in candidates)
            {
                var salt = Convert.FromBase64String(s.RefreshTokenSalt);
                var computedB64 = HashingHelper.HashPassword(rawRefresh, salt);
                if (CryptographicOperations.FixedTimeEquals(
                        Convert.FromBase64String(computedB64),
                        Convert.FromBase64String(s.RefreshTokenHash)))
                {
                    session = s;
                    break;
                }
            }

            if (session is null)
                throw new UnauthorizedException("Invalid refresh token");

            session.RevokedAtUtc = DateTime.UtcNow;

            var newRaw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var newSaltBytes = HashingHelper.GenerateSalt();
            var newHashB64 = HashingHelper.HashPassword(newRaw, newSaltBytes);
            var newSaltB64 = Convert.ToBase64String(newSaltBytes);

            session.ReplacedByTokenHash = newHashB64;

            var newSession = new Session
            {
                RefreshTokenHash = newHashB64,
                RefreshTokenSalt = newSaltB64,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwt.RefreshDays),
                UserId = session.UserId,
                User = session.User
            };

            await _dbContext.Sessions.AddAsync(newSession, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var newAccessToken = _tokenService.CreateAccessToken(session.User);
            SetJwtCookie(response, newAccessToken, DateTimeOffset.UtcNow.AddMinutes(_jwt.DurationInMinutes));
            SetRefreshCookie(response, newRaw, newSession.ExpiresAtUtc);
            SetCsrfCookie(response);
        }

        public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken)
        {
            var sessions = await _dbContext.Sessions
                .Where(s => s.UserId == userId && s.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

            if (sessions.Count == 0)
                throw new NotFoundException("No active sessions found for the user.");

            foreach (var session in sessions)
                session.RevokedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private void SetJwtCookie(HttpResponse response, string accessToken, DateTimeOffset expires)
        {
            response.Cookies.Append(_jwt.JwtCookieName, accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                Expires = expires,
                Domain = _jwt.CookieDomain
            });
        }

        private void SetRefreshCookie(HttpResponse response, string rawRefresh, DateTime expiresUtc)
        {
            response.Cookies.Append(_jwt.RefreshCookieName, rawRefresh, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                Expires = new DateTimeOffset(expiresUtc),
                Path = "/api/auth/refresh",
                Domain = _jwt.CookieDomain
            });
        }

        private void SetCsrfCookie(HttpResponse response)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            response.Cookies.Append(_jwt.CsrfCookieName, token, new CookieOptions
            {
                HttpOnly = false,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                Path = "/",
                Domain = _jwt.CookieDomain
            });
            response.Headers[_jwt.CsrfHeaderName] = token;
        }
    }
}