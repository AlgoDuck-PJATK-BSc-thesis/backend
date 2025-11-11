using AlgoDuck.DAL;
using AlgoDuck.Models.User;
using AlgoDuck.Modules.Auth.DTOs;
using AlgoDuck.Modules.Auth.Interfaces;
using AlgoDuck.Modules.Auth.Jwt;
using AlgoDuck.Models.Auth;
using AlgoDuck.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace AlgoDuck.Modules.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _dbContext;
        private readonly JwtSettings _jwtSettings;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            ApplicationDbContext dbContext,
            IOptions<JwtSettings> options,
            IWebHostEnvironment env,
            IHttpContextAccessor http,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _dbContext = dbContext;
            _jwtSettings = options.Value;
            _env = env;
            _http = http;
            _logger = logger;
        }

        public async Task RegisterAsync(RegisterDto dto, CancellationToken cancellationToken)
        {
            if (await _userManager.FindByNameAsync(dto.Username) != null)
                throw new UsernameAlreadyExistsException();

            if (await _userManager.FindByEmailAsync(dto.Email) is not null)
                throw new EmailAlreadyExistsException();

            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                CohortId = null,
                Coins = 0,
                Experience = 0,
                AmountSolved = 0,
                LockoutEnabled = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                var httpCtx = _http.HttpContext;
                _logger.LogWarning(
                    "auth_register_failed username={Username} email={Email} ip={IP} ua={UA} reason={Reason}",
                    dto.Username,
                    dto.Email,
                    httpCtx?.Connection.RemoteIpAddress?.ToString(),
                    httpCtx?.Request.Headers["User-Agent"].ToString(),
                    msg);
                throw new ValidationException(msg);
            }

            await _userManager.AddToRoleAsync(user, "user");

            var httpCtx2 = _http.HttpContext;
            _logger.LogInformation(
                "auth_register_success user={UserId} ip={IP} ua={UA}",
                user.Id,
                httpCtx2?.Connection.RemoteIpAddress?.ToString(),
                httpCtx2?.Request.Headers["User-Agent"].ToString());
        }

        public async Task<(string AccessToken, string RefreshToken)> LoginAsync(LoginDto dto, HttpResponse response, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == dto.Username || u.Email == dto.Username, cancellationToken);

            if (user == null)
                throw new UserNotFoundException();

            if (await _userManager.IsLockedOutAsync(user))
                throw new ForbiddenException("Account is locked. Try again later.");

            var passwordOk = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordOk)
            {
                await _userManager.AccessFailedAsync(user);

                if (await _userManager.IsLockedOutAsync(user))
                    throw new ForbiddenException("Account is locked. Try again later.");

                var httpCtx = _http.HttpContext;
                _logger.LogWarning(
                    "auth_login_failed_bad_password user={UserId} ip={IP} ua={UA}",
                    user.Id,
                    httpCtx?.Connection.RemoteIpAddress?.ToString(),
                    httpCtx?.Request.Headers["User-Agent"].ToString());

                throw new UnauthorizedException("Invalid password.");
            }

            if (!_env.IsDevelopment() && !await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogWarning(
                    "auth_login_blocked_unconfirmed_email user={UserId} ip={IP} ua={UA}",
                    user.Id,
                    _http.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                    _http.HttpContext?.Request.Headers["User-Agent"].ToString());
                throw new ForbiddenException("Please verify your email before logging in.");
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var accessToken = await _tokenService.CreateAccessTokenAsync(user);

            var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var saltBytes = Shared.Utilities.HashingHelper.GenerateSalt();
            var hashB64 = Shared.Utilities.HashingHelper.HashPassword(rawRefresh, saltBytes);
            var saltB64 = Convert.ToBase64String(saltBytes);

            var session = new Session
            {
                RefreshTokenHash = hashB64,
                RefreshTokenSalt = saltB64,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshDays),
                UserId = user.Id,
                User = user
            };

            await _dbContext.Sessions.AddAsync(session, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            SetJwtCookie(response, accessToken, DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes));
            SetRefreshCookie(response, rawRefresh, session.ExpiresAtUtc);
            SetCsrfCookie(response);

            var httpCtx2 = _http.HttpContext;
            _logger.LogInformation(
                "auth_login_success user={UserId} ip={IP} ua={UA}",
                user.Id,
                httpCtx2?.Connection.RemoteIpAddress?.ToString(),
                httpCtx2?.Request.Headers["User-Agent"].ToString());

            return (accessToken, rawRefresh);
        }

        public async Task RefreshTokenAsync(RefreshDto dto, HttpResponse response, CancellationToken cancellationToken)
    {
        var ctx = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context");

        string? rawRefresh = dto.RefreshToken;
        if (string.IsNullOrWhiteSpace(rawRefresh))
        {
            var cookies = ctx.Request.Cookies;
            if (!cookies.TryGetValue(_jwtSettings.RefreshCookieName, out rawRefresh) || string.IsNullOrWhiteSpace(rawRefresh))
                throw new UnauthorizedException("Missing refresh token");

            var header = ctx.Request.Headers[_jwtSettings.CsrfHeaderName].ToString();
            if (!cookies.TryGetValue(_jwtSettings.CsrfCookieName, out var csrfCookie) || csrfCookie != header)
                throw new ForbiddenException("CSRF validation failed");
        }

        var revoked = await _dbContext.Sessions
            .AsNoTracking()
            .Where(s => s.RevokedAtUtc != null)
            .Select(s => new { s.SessionId, s.UserId, s.RefreshTokenHash, s.RefreshTokenSalt })
            .ToListAsync(cancellationToken);

        var reused = revoked.FirstOrDefault(s => MatchesRefresh(s.RefreshTokenHash, s.RefreshTokenSalt, rawRefresh));
        if (reused is not null)
        {
            _logger.LogWarning(
                "auth_refresh_reuse_detected user={UserId} session={SessionId} ip={IP} ua={UA}",
                reused.UserId,
                reused.SessionId,
                ctx.Connection.RemoteIpAddress?.ToString(),
                ctx.Request.Headers["User-Agent"].ToString());

            var now = DateTime.UtcNow;
            var activeForUser = await _dbContext.Sessions
                .Where(x => x.UserId == reused.UserId && x.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var a in activeForUser)
                a.RevokedAtUtc = now;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var domainR = string.IsNullOrWhiteSpace(_jwtSettings.CookieDomain) ? null : _jwtSettings.CookieDomain;
            response.Cookies.Delete(_jwtSettings.JwtCookieName, new CookieOptions { Path = "/", Domain = domainR });
            response.Cookies.Delete(_jwtSettings.RefreshCookieName, new CookieOptions { Path = "/api/auth/refresh", Domain = domainR });
            response.Cookies.Delete(_jwtSettings.CsrfCookieName, new CookieOptions { Path = "/", Domain = domainR });

            throw new UnauthorizedException("Refresh token reuse detected. All sessions revoked.");
        }

        var candidates = await _dbContext.Sessions
            .Include(s => s.User)
            .Where(s => s.RevokedAtUtc == null && s.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        Session? session = null;
        foreach (var s in candidates)
        {
            if (MatchesRefresh(s.RefreshTokenHash, s.RefreshTokenSalt, rawRefresh))
            {
                session = s;
                break;
            }
        }

        if (session is null)
            throw new UnauthorizedException("Invalid refresh token");

        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            session.RevokedAtUtc = DateTime.UtcNow;

            var newRaw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var newSaltBytes = Shared.Utilities.HashingHelper.GenerateSalt();
            var newHashB64 = Shared.Utilities.HashingHelper.HashPassword(newRaw, newSaltBytes);
            var newSaltB64 = Convert.ToBase64String(newSaltBytes);

            var newId = Guid.NewGuid();

            var newSession = new Session
            {
                SessionId = newId,
                RefreshTokenHash = newHashB64,
                RefreshTokenSalt = newSaltB64,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshDays),
                UserId = session.UserId,
                User = session.User
            };

            session.ReplacedBySessionId = newId;

            _dbContext.Sessions.Add(newSession);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            var newAccessToken = await _tokenService.CreateAccessTokenAsync(session.User);
            SetJwtCookie(response, newAccessToken, DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes));
            SetRefreshCookie(response, newRaw, newSession.ExpiresAtUtc);
            SetCsrfCookie(response);

            _logger.LogInformation(
                "auth_refresh_success user={UserId} session={SessionId} ip={IP} ua={UA}",
                session.UserId,
                newId,
                ctx.Connection.RemoteIpAddress?.ToString(),
                ctx.Request.Headers["User-Agent"].ToString());
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

        public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken)
        {
            var sessions = await _dbContext.Sessions
                .Where(s => s.UserId == userId && s.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

            if (sessions.Count == 0)
                throw new NotFoundException("No active sessions found for the user.");

            foreach (var s in sessions)
                s.RevokedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var domain = string.IsNullOrWhiteSpace(_jwtSettings.CookieDomain) ? null : _jwtSettings.CookieDomain;
            _http.HttpContext?.Response.Cookies.Delete(_jwtSettings.JwtCookieName, new CookieOptions { Path = "/", Domain = domain });
            _http.HttpContext?.Response.Cookies.Delete(_jwtSettings.RefreshCookieName, new CookieOptions { Path = "/api/auth/refresh", Domain = domain });
            _http.HttpContext?.Response.Cookies.Delete(_jwtSettings.CsrfCookieName, new CookieOptions { Path = "/", Domain = domain });

            var httpCtx = _http.HttpContext;
            _logger.LogInformation(
                "auth_logout user={UserId} ip={IP} ua={UA}",
                userId,
                httpCtx?.Connection.RemoteIpAddress?.ToString(),
                httpCtx?.Request.Headers["User-Agent"].ToString());
        }

        private static bool MatchesRefresh(string storedHashB64, string storedSaltB64, string rawRefresh)
        {
            var salt = Convert.FromBase64String(storedSaltB64);
            var computedB64 = Shared.Utilities.HashingHelper.HashPassword(rawRefresh, salt);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(computedB64),
                Convert.FromBase64String(storedHashB64));
        }

        private void SetJwtCookie(HttpResponse response, string accessToken, DateTimeOffset expires)
        {
            var domain = string.IsNullOrWhiteSpace(_jwtSettings.CookieDomain) ? null : _jwtSettings.CookieDomain;
            response.Cookies.Append(_jwtSettings.JwtCookieName, accessToken, new CookieOptions
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
            var domain = string.IsNullOrWhiteSpace(_jwtSettings.CookieDomain) ? null : _jwtSettings.CookieDomain;
            response.Cookies.Append(_jwtSettings.RefreshCookieName, rawRefresh, new CookieOptions
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
            var domain = string.IsNullOrWhiteSpace(_jwtSettings.CookieDomain) ? null : _jwtSettings.CookieDomain;
            response.Cookies.Append(_jwtSettings.CsrfCookieName, token, new CookieOptions
            {
                HttpOnly = false,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                Path = "/",
                Domain = domain
            });
            response.Headers[_jwtSettings.CsrfHeaderName] = token;
        }
    }
}