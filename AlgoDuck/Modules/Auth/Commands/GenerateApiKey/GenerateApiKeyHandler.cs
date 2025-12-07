namespace AlgoDuck.Modules.Auth.Commands.GenerateApiKey;

using System;
using System.Threading;
using System.Threading.Tasks;
using Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using FluentValidation;

public sealed class GenerateApiKeyHandler : IGenerateApiKeyHandler
{
    private readonly IApiKeyService _apiKeyService;
    private readonly IValidator<GenerateApiKeyDto> _validator;

    public GenerateApiKeyHandler(
        IApiKeyService apiKeyService,
        IValidator<GenerateApiKeyDto> validator)
    {
        _apiKeyService = apiKeyService;
        _validator = validator;
    }

    public async Task<GenerateApiKeyResult> HandleAsync(Guid userId, GenerateApiKeyDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        if (userId == Guid.Empty)
        {
            throw new ApiKeyException("User is not authenticated.");
        }

        TimeSpan? lifetime = null;
        if (dto.LifetimeDays.HasValue)
        {
            lifetime = TimeSpan.FromDays(dto.LifetimeDays.Value);
        }

        var creationResult = await _apiKeyService.CreateApiKeyAsync(userId, dto.Name, lifetime, cancellationToken);

        return new GenerateApiKeyResult
        {
            ApiKey = creationResult.ApiKey,
            RawKey = creationResult.RawKey
        };
    }
}