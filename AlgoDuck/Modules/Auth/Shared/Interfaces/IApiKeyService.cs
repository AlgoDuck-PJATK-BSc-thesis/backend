namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DTOs;

public interface IApiKeyService
{
    Task<ApiKeyCreationResult> CreateApiKeyAsync(Guid userId, string name, TimeSpan? lifetime, CancellationToken cancellationToken);
    Task<IReadOnlyList<ApiKeyDto>> GetUserApiKeysAsync(Guid userId, CancellationToken cancellationToken);
    Task RevokeApiKeyAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken);
    Task<Guid> ValidateAndGetUserIdAsync(string rawApiKey, CancellationToken cancellationToken);
}