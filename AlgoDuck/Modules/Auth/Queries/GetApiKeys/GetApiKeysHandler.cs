using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Queries.GetApiKeys;

public sealed class GetApiKeysHandler : IGetApiKeysHandler
{
    private readonly IApiKeyService _apiKeyService;

    public GetApiKeysHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<IReadOnlyList<ApiKeyDto>> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ApiKeyException("User identifier is invalid.");
        }

        return await _apiKeyService.GetUserApiKeysAsync(userId, cancellationToken);
    }
}