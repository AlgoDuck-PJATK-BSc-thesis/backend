using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.Email.VerifyEmail;

public static class VerifyEmailEndpoint
{
    public static IEndpointRouteBuilder MapVerifyEmailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/email-verification", VerifyEmail)
            .WithTags("Auth");

        app.MapGet("/auth/email-verification", VerifyEmailFromQuery)
            .WithTags("Auth");

        app.MapGet("/auth/email-verification/success.js", SuccessJs)
            .WithTags("Auth");

        return app;
    }

    private static async Task<IResult> VerifyEmail(
        [FromBody] VerifyEmailDto request,
        IVerifyEmailHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(request, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> VerifyEmailFromQuery(
        [FromQuery] Guid userId,
        [FromQuery] string token,
        [FromQuery] string? returnUrl,
        IVerifyEmailHandler handler,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var dto = new VerifyEmailDto
        {
            UserId = userId,
            Token = token
        };

        try
        {
            await handler.HandleAsync(dto, cancellationToken);
        }
        catch (Exception ex)
        {
            return Results.Content(
                BuildFailureHtml(ex.Message),
                "text/html",
                Encoding.UTF8,
                (int)HttpStatusCode.BadRequest);
        }

        var frontendBaseUrl = GetFrontendBaseUrl(configuration);
        var safeRedirect = BuildRedirectUrl(returnUrl, configuration, frontendBaseUrl);

        return Results.Content(
            BuildSuccessHtml(frontendBaseUrl, safeRedirect),
            "text/html",
            Encoding.UTF8,
            (int)HttpStatusCode.OK);
    }

    private static IResult SuccessJs([FromQuery] string? returnUrl)
    {
        var js = string.IsNullOrWhiteSpace(returnUrl)
            ? string.Empty
            : """
              (() => {
                try {
                  const s = document.currentScript;
                  if (!s) return;
                  const u = new URL(s.src);
                  const r = u.searchParams.get("returnUrl");
                  if (!r) return;

                  const controller = new AbortController();
                  const t = setTimeout(() => controller.abort(), 700);

                  fetch(r, { method: "GET", mode: "no-cors", cache: "no-store", signal: controller.signal })
                    .then(() => {
                      clearTimeout(t);
                      window.location.replace(r);
                    })
                    .catch(() => {});
                } catch (_) {}
              })();
              """;

        return Results.Text(js, "application/javascript; charset=utf-8");
    }

    private static string GetFrontendBaseUrl(IConfiguration configuration)
    {
        if (configuration["ASPNETCORE_ENVIRONMENT"]?.Equals("Production", StringComparison.OrdinalIgnoreCase) == true)
            return "https://algoduck.pl";
    
        var v =
            configuration["App:FrontendUrl"] ??
            configuration["CORS:DevOrigins:0"] ??
            "http://localhost:5173";

        return v.TrimEnd('/');
    }

    private static string BuildRedirectUrl(string? returnUrl, IConfiguration configuration, string frontendBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return frontendBaseUrl;
        }

        if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var absolute))
        {
            return IsAllowedAbsolute(absolute, configuration, frontendBaseUrl) ? absolute.ToString() : frontendBaseUrl;
        }

        if (!returnUrl.StartsWith("/") || returnUrl.StartsWith("//"))
        {
            return frontendBaseUrl;
        }

        return frontendBaseUrl + returnUrl;
    }

    private static bool IsAllowedAbsolute(Uri absolute, IConfiguration configuration, string frontendBaseUrl)
    {
        foreach (var origin in GetAllowedOrigins(configuration, frontendBaseUrl))
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var allowed))
            {
                continue;
            }

            var sameScheme = string.Equals(absolute.Scheme, allowed.Scheme, StringComparison.OrdinalIgnoreCase);
            var sameHost = string.Equals(absolute.Host, allowed.Host, StringComparison.OrdinalIgnoreCase);
            var samePort = absolute.Port == allowed.Port;

            if (sameScheme && sameHost && samePort)
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> GetAllowedOrigins(IConfiguration configuration, string frontendBaseUrl)
    {
        yield return frontendBaseUrl;

        foreach (var o in GetIndexed(configuration.GetSection("CORS:DevOrigins")))
        {
            yield return o;
        }

        foreach (var o in GetIndexed(configuration.GetSection("CORS:ProdOrigins")))
        {
            yield return o;
        }
    }

    private static IEnumerable<string> GetIndexed(IConfigurationSection section)
    {
        for (var i = 0; i < 50; i++)
        {
            var v = section[i.ToString()];
            if (string.IsNullOrWhiteSpace(v))
            {
                yield break;
            }

            yield return v.TrimEnd('/');
        }
    }

    private static string BuildSuccessHtml(string frontendBaseUrl, string? redirectUrl)
    {
        var safeRedirect = WebUtility.HtmlEncode(redirectUrl ?? frontendBaseUrl);
        var encodedRedirect = Uri.EscapeDataString(redirectUrl ?? frontendBaseUrl);

        return $"""
                <!doctype html>
                <html lang="en">
                <head>
                  <meta charset="utf-8">
                  <meta name="viewport" content="width=device-width,initial-scale=1">
                  <title>Email confirmed</title>
                </head>
                <body>
                  <h1>Your email address has been confirmed</h1>
                  <p>You will be automatically re-directed now.</p>
                  <p><a href="{safeRedirect}">Continue to AlgoDuck</a></p>
                  <p>If nothing happens automatically, click the link above.</p>
                  <script src="/auth/email-verification/success.js?returnUrl={encodedRedirect}"></script>
                </body>
                </html>
                """;
    }

    private static string BuildFailureHtml(string errorMessage)
    {
        var msg = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(errorMessage) ? "Invalid token." : errorMessage);

        return $"""
                <!doctype html>
                <html lang="en">
                <head>
                  <meta charset="utf-8">
                  <meta name="viewport" content="width=device-width,initial-scale=1">
                  <title>Email confirmation failed</title>
                </head>
                <body>
                  <h1>Email confirmation failed</h1>
                  <p>{msg}</p>
                  <p>Please go back to AlgoDuck and request a new confirmation email.</p>
                </body>
                </html>
                """;
    }
}
