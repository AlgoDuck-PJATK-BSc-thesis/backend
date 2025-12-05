using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface IApiKeyService
{
    Task<ApiKeyDto> CreateApiKeyAsync(Guid userId, string name, TimeSpan? lifetime, CancellationToken cancellationToken);
    Task<IReadOnlyList<ApiKeyDto>> GetUserApiKeysAsync(Guid userId, CancellationToken cancellationToken);
    Task RevokeApiKeyAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken);
    Task<Guid> ValidateAndGetUserIdAsync(string rawApiKey, CancellationToken cancellationToken);
}