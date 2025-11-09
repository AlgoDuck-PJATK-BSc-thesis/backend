using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace AlgoDuck.Shared.Middleware;

public sealed class SecurityHeaders
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;
    private readonly SecurityHeadersOptions _opt;

    public SecurityHeaders(RequestDelegate next, IHostEnvironment env, IOptions<SecurityHeadersOptions> opt)
    {
        _next = next;
        _env = env;
        _opt = opt.Value;
    }

    public async Task Invoke(HttpContext ctx)
    {
        ctx.Response.Headers[HeaderNames.XContentTypeOptions] = "nosniff";
        ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
        ctx.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        ctx.Response.Headers["X-Frame-Options"] = "DENY";

        var connect = new List<string> { "'self'" };
        connect.AddRange(_opt.ConnectOrigins);
        if (_env.IsDevelopment())
        {
            connect.Add("http://localhost:5173");
            connect.Add("ws://localhost:5173");
        }

        var img = new List<string> { "'self'", "data:", "blob:" };
        img.AddRange(_opt.ImgOrigins);

        var style = new List<string> { "'self'" };
        if (_env.IsDevelopment()) style.Add("'unsafe-inline'");
        style.AddRange(_opt.StyleOrigins);

        var font = new List<string> { "'self'", "data:" };
        font.AddRange(_opt.FontOrigins);

        var csp = string.Join("; ",
            "default-src 'self'",
            "base-uri 'self'",
            "frame-ancestors 'none'",
            "object-src 'none'",
            $"connect-src {string.Join(" ", connect)}",
            $"img-src {string.Join(" ", img)}",
            $"font-src {string.Join(" ", font)}",
            "script-src 'self'",
            $"style-src {string.Join(" ", style)}",
            "form-action 'self'"
        );

        if (_env.IsDevelopment() && ctx.Request.Path.StartsWithSegments("/swagger"))
        {
            var swaggerCsp = string.Join("; ",
                "default-src 'self'",
                "base-uri 'self'",
                "frame-ancestors 'none'",
                "object-src 'none'",
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'",
                "style-src 'self' 'unsafe-inline'",
                $"img-src {string.Join(" ", img)}",
                $"font-src {string.Join(" ", font)}",
                $"connect-src {string.Join(" ", connect)}",
                "form-action 'self'"
            );

            var headerSwagger = _opt.ReportOnly ? "Content-Security-Policy-Report-Only" : "Content-Security-Policy";
            ctx.Response.Headers[headerSwagger] = swaggerCsp;
        }
        else
        {
            var header = _opt.ReportOnly ? "Content-Security-Policy-Report-Only" : "Content-Security-Policy";
            ctx.Response.Headers[header] = csp;
        }

        await _next(ctx);
    }
}