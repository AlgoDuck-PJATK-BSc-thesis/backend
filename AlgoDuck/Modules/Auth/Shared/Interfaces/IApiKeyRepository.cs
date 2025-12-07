using AlgoDuck.Models;

namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface IApiKeyRepository
{
    Task AddAsync(ApiKey apiKey, CancellationToken cancellationToken);
    Task<ApiKey?> GetByIdAsync(Guid apiKeyId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ApiKey>> GetUserApiKeysAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ApiKey>> FindActiveByPrefixAsync(string prefix, DateTimeOffset now, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}