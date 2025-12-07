using System.Security.Cryptography;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Mappers;
using AlgoDuck.Modules.Auth.Shared.Utils;
using AlgoDuck.Modules.Auth.Shared.Validators;
using AlgoDuck.Shared.Utilities;

namespace AlgoDuck.Modules.Auth.Shared.Services;

public sealed class ApiKeyService : IApiKeyService
{
    private const int PrefixLength = 16;

    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IAuthRepository _authRepository;
    private readonly ApiKeyValidator _validator;

    public ApiKeyService(
        IApiKeyRepository apiKeyRepository,
        IAuthRepository authRepository,
        ApiKeyValidator validator)
    {
        _apiKeyRepository = apiKeyRepository;
        _authRepository = authRepository;
        _validator = validator;
    }

    public async Task<ApiKeyCreationResult> CreateApiKeyAsync(Guid userId, string name, TimeSpan? lifetime, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ApiKeyException("User identifier is invalid.");
        }

        _validator.ValidateName(name);

        var user = await _authRepository.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new ApiKeyException("User not found.");
        }

        var material = ApiKeyGenerator.Generate();

        var now = DateTimeOffset.UtcNow;
        DateTimeOffset? expiresAt = null;
        if (lifetime.HasValue && lifetime.Value > TimeSpan.Zero)
        {
            expiresAt = now.Add(lifetime.Value);
        }

        var apiKey = new ApiKey
        {
            UserId = user.Id,
            Name = name,
            Prefix = material.Prefix,
            KeyHash = material.Hash,
            KeySalt = material.Salt,
            CreatedAt = now,
            ExpiresAt = expiresAt
        };

        await _apiKeyRepository.AddAsync(apiKey, cancellationToken);
        await _apiKeyRepository.SaveChangesAsync(cancellationToken);

        var dto = ApiKeyMapper.ToApiKeyDto(apiKey);

        return new ApiKeyCreationResult
        {
            ApiKey = dto,
            RawKey = material.RawKey
        };
    }

    public async Task<IReadOnlyList<ApiKeyDto>> GetUserApiKeysAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ApiKeyException("User identifier is invalid.");
        }

        var apiKeys = await _apiKeyRepository.GetUserApiKeysAsync(userId, cancellationToken);

        return apiKeys
            .Select(ApiKeyMapper.ToApiKeyDto)
            .ToList();
    }

    public async Task RevokeApiKeyAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ApiKeyException("User identifier is invalid.");
        }

        var apiKey = await _apiKeyRepository.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey is null)
        {
            throw new ApiKeyException("API key not found.");
        }

        if (apiKey.UserId != userId)
        {
            throw new PermissionException("You do not own this API key.");
        }

        if (apiKey.RevokedAt.HasValue)
        {
            return;
        }

        apiKey.RevokedAt = DateTimeOffset.UtcNow;
        await _apiKeyRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> ValidateAndGetUserIdAsync(string rawApiKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawApiKey))
        {
            throw new ApiKeyException("API key is required.");
        }

        var prefixLength = Math.Min(rawApiKey.Length, PrefixLength);
        var prefix = rawApiKey.Substring(0, prefixLength);
        var now = DateTimeOffset.UtcNow;

        var candidates = await _apiKeyRepository.FindActiveByPrefixAsync(prefix, now, cancellationToken);

        foreach (var candidate in candidates)
        {
            var saltBytes = Convert.FromBase64String(candidate.KeySalt);
            var computedB64 = HashingHelper.HashPassword(rawApiKey, saltBytes);
            var storedBytes = Convert.FromBase64String(candidate.KeyHash);
            var computedBytes = Convert.FromBase64String(computedB64);

            var match = CryptographicOperations.FixedTimeEquals(storedBytes, computedBytes);
            if (match)
            {
                return candidate.UserId;
            }
        }

        throw new ApiKeyException("API key is invalid.");
    }
}