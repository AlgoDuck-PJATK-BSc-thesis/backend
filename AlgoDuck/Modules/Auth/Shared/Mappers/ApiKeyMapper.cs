using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Shared.Mappers;

public static class ApiKeyMapper
{
    public static ApiKeyDto ToApiKeyDto(ApiKey apiKey)
    {
        return new ApiKeyDto
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            Prefix = apiKey.Prefix,
            CreatedAt = apiKey.CreatedAt,
            ExpiresAt = apiKey.ExpiresAt,
            IsRevoked = apiKey.RevokedAt.HasValue
        };
    }
}