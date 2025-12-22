using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Modules.Auth.Commands.RevokeApiKey;

public sealed class RevokeApiKeyHandler : IRevokeApiKeyHandler
{
    private readonly IApiKeyService _apiKeyService;

    public RevokeApiKeyHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task HandleAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ApiKeyException("User is not authenticated.");
        }

        if (apiKeyId == Guid.Empty)
        {
            throw new ApiKeyException("API key identifier is invalid.");
        }

        await _apiKeyService.RevokeApiKeyAsync(userId, apiKeyId, cancellationToken);
    }
}