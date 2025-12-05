using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.RevokeApiKey;

public sealed class RevokeApiKeyHandler : IRevokeApiKeyHandler
{
    private readonly IApiKeyService _apiKeyService;
    private readonly IValidator<RevokeApiKeyDto> _validator;

    public RevokeApiKeyHandler(
        IApiKeyService apiKeyService,
        IValidator<RevokeApiKeyDto> validator)
    {
        _apiKeyService = apiKeyService;
        _validator = validator;
    }

    public async Task HandleAsync(Guid userId, Guid apiKeyId, RevokeApiKeyDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

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