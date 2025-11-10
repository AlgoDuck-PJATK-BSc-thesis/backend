using System.Security.Cryptography;
using System.Text;
using AlgoDuck.Modules.Auth.Jwt;
using AlgoDuck.Shared.Http;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Shared.Middleware;

public sealed class CsrfGuard
{
    private static readonly HashSet<string> Safe = new(StringComparer.OrdinalIgnoreCase)
        { "GET", "HEAD", "OPTIONS", "TRACE" };

    private static readonly string[] AllowUnauthenticatedPosts =
    {
        "/api/auth/login-start",
        "/api/auth/login-verify",
        "/api/auth/refresh"
    };

    private readonly RequestDelegate _next;
    private readonly JwtSettings _jwt;
    private readonly IHostEnvironment _env;
    private readonly ILogger<CsrfGuard> _logger;

    public CsrfGuard(
        RequestDelegate next,
        IOptions<JwtSettings> jwtOptions,
        IHostEnvironment env,
        ILogger<CsrfGuard> logger)
    {
        _next = next;
        _jwt = jwtOptions.Value;
        _env = env;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx)
    {
        if (Safe.Contains(ctx.Request.Method))
        {
            await _next(ctx);
            return;
        }

        var path = ctx.Request.Path.Value ?? string.Empty;

        if (ctx.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            if (AllowUnauthenticatedPosts.Contains(path, StringComparer.OrdinalIgnoreCase) ||
                path.StartsWith("/api/auth/oauth/", StringComparison.OrdinalIgnoreCase))
            {
                await _next(ctx);
                return;
            }
        }

        if (path.Contains("/negotiate", StringComparison.OrdinalIgnoreCase))
        {
            await _next(ctx);
            return;
        }

        if (_env.IsDevelopment() && path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(ctx);
            return;
        }

        var hasCredCookies =
            ctx.Request.Cookies.ContainsKey(_jwt.JwtCookieName) ||
            ctx.Request.Cookies.ContainsKey(_jwt.RefreshCookieName);

        if (!hasCredCookies)
        {
            await _next(ctx);
            return;
        }

        var cookieVal = ctx.Request.Cookies[_jwt.CsrfCookieName];
        var headerVal = ctx.Request.Headers[_jwt.CsrfHeaderName].ToString();

        if (!TimeSafeEquals(cookieVal, headerVal))
        {
            _logger.LogWarning(
                "CSRF validation failed for {Method} {Path} from {IP}. HasCookie={HasCookie} HasHeader={HasHeader}",
                ctx.Request.Method,
                path,
                ctx.Connection.RemoteIpAddress?.ToString(),
                !string.IsNullOrEmpty(cookieVal),
                !string.IsNullOrEmpty(headerVal));

            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            await ctx.Response.WriteAsJsonAsync(
                ApiResponse.Fail("CSRF validation failed.", "csrf_failed"));
            return;
        }

        await _next(ctx);
    }

    private static bool TimeSafeEquals(string? a, string? b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
        var ab = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(ab, bb);
    }
}