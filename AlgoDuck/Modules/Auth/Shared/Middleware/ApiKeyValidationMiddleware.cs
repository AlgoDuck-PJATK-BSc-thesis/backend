using AlgoDuck.Modules.Auth.Shared.Constants;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Modules.Auth.Shared.Middleware;

public sealed class ApiKeyValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiKeyService _apiKeyService;

    public ApiKeyValidationMiddleware(RequestDelegate next, IApiKeyService apiKeyService)
    {
        _next = next;
        _apiKeyService = apiKeyService;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderConstants.ApiKeyHeaderName, out var values))
        {
            await _next(context);
            return;
        }

        var apiKey = values.ToString();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            await _next(context);
            return;
        }

        try
        {
            var userId = await _apiKeyService.ValidateAndGetUserIdAsync(apiKey, context.RequestAborted);
            context.Items[ApiKeyHeaderConstants.ApiKeyUserIdItemKey] = userId;
        }
        catch (ApiKeyException)
        {
        }

        await _next(context);
    }
}