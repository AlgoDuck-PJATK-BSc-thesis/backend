using Microsoft.Net.Http.Headers;

namespace AlgoDuck.Shared.Middleware;

public sealed class SecurityHeaders
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    public SecurityHeaders(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task Invoke(HttpContext ctx)
    {
        ctx.Response.Headers[HeaderNames.XContentTypeOptions] = "nosniff";
        ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
        ctx.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

        if (_env.IsDevelopment())
        {
            var csp = string.Join("; ",
                "default-src 'self'",
                "base-uri 'self'",
                "frame-ancestors 'none'",
                "object-src 'none'",
                "connect-src 'self' http://localhost:5173 ws: wss:",
                "img-src 'self' data: blob:",
                "font-src 'self' data:",
                "script-src 'self'",
                "style-src 'self' 'unsafe-inline'"
            );
            ctx.Response.Headers["Content-Security-Policy"] = csp;
        }
        else
        {
            var csp = string.Join("; ",
                "default-src 'self'",
                "base-uri 'self'",
                "frame-ancestors 'none'",
                "object-src 'none'",
                "connect-src 'self' https://algoduck.com wss:",
                "img-src 'self' data: blob:",
                "font-src 'self' data:",
                "script-src 'self'",
                "style-src 'self'"
            );
            ctx.Response.Headers["Content-Security-Policy"] = csp;
        }

        await _next(ctx);
    }
}