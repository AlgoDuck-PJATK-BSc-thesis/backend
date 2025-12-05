using AlgoDuck.DAL;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Auth.Queries.SearchUsersByEmail;

public sealed class SearchUsersByEmailHandler : ISearchUsersByEmailHandler
{
    private readonly ApplicationCommandDbContext _dbContext;

    public SearchUsersByEmailHandler(ApplicationCommandDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AuthUserDto>> HandleAsync(SearchUsersByEmailDto dto, CancellationToken cancellationToken)
    {
        var normalizedQuery = dto.Query.Trim().ToLowerInvariant();
        var limit = dto.Limit <= 0 ? 20 : Math.Min(dto.Limit, 100);

        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return Array.Empty<AuthUserDto>();
        }

        var users = await _dbContext.ApplicationUsers
            .AsNoTracking()
            .Where(u => u.Email != null && u.Email.ToLower().Contains(normalizedQuery))
            .OrderBy(u => u.Email)
            .Take(limit)
            .Select(u => new AuthUserDto
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                EmailConfirmed = u.EmailConfirmed
            })
            .ToListAsync(cancellationToken);

        return users;
    }
}