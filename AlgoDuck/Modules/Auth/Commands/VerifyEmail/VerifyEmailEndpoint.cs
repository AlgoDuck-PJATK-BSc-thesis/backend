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
        IVerifyEmailHandler handler,
        CancellationToken cancellationToken)
    {
        var dto = new VerifyEmailDto
        {
            UserId = userId,
            Token = token
        };

        await handler.HandleAsync(dto, cancellationToken);

        const string successHtml =
            "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Email verified</title></head><body><h1>Email verified</h1><p>Your email address has been successfully verified. You can now close this tab and return to AlgoDuck.</p></body></html>";

        return Results.Content(successHtml, "text/html");
    }
}