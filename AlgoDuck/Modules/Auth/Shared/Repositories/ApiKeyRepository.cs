using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Auth.Shared.Repositories;

public sealed class ApiKeyRepository : IApiKeyRepository
{
    private readonly ApplicationCommandDbContext _commandDbContext;

    public ApiKeyRepository(ApplicationCommandDbContext commandDbContext)
    {
        _commandDbContext = commandDbContext;
    }

    public async Task AddAsync(ApiKey apiKey, CancellationToken cancellationToken)
    {
        await _commandDbContext.Set<ApiKey>().AddAsync(apiKey, cancellationToken);
    }

    public async Task<ApiKey?> GetByIdAsync(Guid apiKeyId, CancellationToken cancellationToken)
    {
        return await _commandDbContext.Set<ApiKey>()
            .FirstOrDefaultAsync(k => k.Id == apiKeyId, cancellationToken);
    }

    public async Task<IReadOnlyList<ApiKey>> GetUserApiKeysAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _commandDbContext.Set<ApiKey>()
            .AsNoTracking()
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ApiKey>> FindActiveByPrefixAsync(string prefix, DateTimeOffset now, CancellationToken cancellationToken)
    {
        return await _commandDbContext.Set<ApiKey>()
            .Where(k =>
                k.Prefix == prefix &&
                !k.RevokedAt.HasValue &&
                (!k.ExpiresAt.HasValue || k.ExpiresAt > now))
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _commandDbContext.SaveChangesAsync(cancellationToken);
    }
}