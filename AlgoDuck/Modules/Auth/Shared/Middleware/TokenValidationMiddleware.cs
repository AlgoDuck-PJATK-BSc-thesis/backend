using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using AlgoDuck.Modules.Auth.Shared.Utils;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Auth.Shared.Middleware;

public sealed class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TokenUtility _tokenUtility;
    private readonly JwtSettings _jwtSettings;

    public TokenValidationMiddleware(
        RequestDelegate next,
        TokenUtility tokenUtility,
        IOptions<JwtSettings> jwtOptions)
    {
        _next = next;
        _tokenUtility = tokenUtility;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context))
        {
            await _next(context);
            return;
        }

        var accessToken = context.Request.Cookies[_jwtSettings.AccessTokenCookieName];
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new TokenException("Access token is missing.");
        }

        var csrfHeader = context.Request.Headers[_jwtSettings.CsrfHeaderName].ToString();
        var csrfCookie = context.Request.Cookies[_jwtSettings.CsrfCookieName];

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