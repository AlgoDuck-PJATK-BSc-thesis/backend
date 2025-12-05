using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Utils;

namespace AlgoDuck.Modules.Auth.Shared.Middleware;

public sealed class TokenValidationMiddleware
{
    private const string AccessTokenCookieName = "access_token";
    private const string CsrfHeaderName = "X-Csrf-Token";
    private const string CsrfCookieName = "csrf_token";

    private readonly RequestDelegate _next;
    private readonly TokenUtility _tokenUtility;

    public TokenValidationMiddleware(
        RequestDelegate next,
        TokenUtility tokenUtility)
    {
        _next = next;
        _tokenUtility = tokenUtility;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context))
        {
            await _next(context);
            return;
        }

        var accessToken = context.Request.Cookies[AccessTokenCookieName];
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new TokenException("Access token is missing.");
        }

        var csrfHeader = context.Request.Headers[CsrfHeaderName].ToString();
        var csrfCookie = context.Request.Cookies[CsrfCookieName];

        if (!_tokenUtility.ValidateCsrf(csrfHeader, csrfCookie))
        {
            throw new TokenException("CSRF token is invalid.");
        }

        await _next(context);
    }

    private static bool ShouldSkip(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method) &&
            !HttpMethods.IsPut(context.Request.Method) &&
            !HttpMethods.IsPatch(context.Request.Method) &&
            !HttpMethods.IsDelete(context.Request.Method))
        {
            return true;
        }

        return false;
    }
}