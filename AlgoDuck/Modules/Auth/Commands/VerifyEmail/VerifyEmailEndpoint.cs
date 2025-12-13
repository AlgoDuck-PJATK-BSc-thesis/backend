using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Commands.VerifyEmail;

public static class VerifyEmailEndpoint
{
    public static IEndpointRouteBuilder MapVerifyEmailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/email-verification", VerifyEmail)
            .WithTags("Auth");

        app.MapGet("/auth/email-verification", VerifyEmailFromQuery)
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
            var errorHtml =
                "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Email verification failed</title></head><body><h1>Email verification failed</h1><p>" +
                WebUtility.HtmlEncode(ex.Message) +
                "</p></body></html>";

            return Results.Content(errorHtml, "text/html", Encoding.UTF8, 400);
        }

        var redirect = BuildRedirectUrl(returnUrl, configuration);
        if (redirect is not null)
        {
            return Results.Redirect(redirect);
        }

        const string successHtml =
            "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Email verified</title></head><body><h1>Email verified</h1><p>Your email address has been successfully verified. You can now close this tab and return to AlgoDuck.</p></body></html>";

        return Results.Content(successHtml, "text/html");
    }

    private static string? BuildRedirectUrl(string? returnUrl, IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return null;
        }

        if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var absolute))
        {
            return IsAllowedAbsolute(absolute, configuration) ? absolute.ToString() : null;
        }

        if (!returnUrl.StartsWith("/") || returnUrl.StartsWith("//"))
        {
            return null;
        }

        var frontendBaseUrl =
            configuration["App:FrontendUrl"] ??
            configuration["CORS:DevOrigins:0"] ??
            "http://localhost:5173";

        return frontendBaseUrl.TrimEnd('/') + returnUrl;
    }

    private static bool IsAllowedAbsolute(Uri absolute, IConfiguration configuration)
    {
        foreach (var origin in GetAllowedOrigins(configuration))
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var allowed))
            {
                continue;
            }

            if (string.Equals(absolute.Scheme, allowed.Scheme, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(absolute.Host, allowed.Host, StringComparison.OrdinalIgnoreCase) &&
                absolute.Port == allowed.Port)
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> GetAllowedOrigins(IConfiguration configuration)
    {
        var frontend = configuration["App:FrontendUrl"];
        if (!string.IsNullOrWhiteSpace(frontend))
        {
            yield return frontend;
        }

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

            yield return v;
        }
    }
}
