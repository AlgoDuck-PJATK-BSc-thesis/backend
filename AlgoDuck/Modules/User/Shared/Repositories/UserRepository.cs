using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Shared.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationQueryDbContext _queryDbContext;
    private readonly ApplicationCommandDbContext _commandDbContext;

    public UserRepository(
        ApplicationQueryDbContext queryDbContext,
        ApplicationCommandDbContext commandDbContext)
    {
        _queryDbContext = queryDbContext;
        _commandDbContext = commandDbContext;
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _queryDbContext.Users
            .Include(u => u.UserConfig)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByNameAsync(string userName, CancellationToken cancellationToken)
    {
        return await _queryDbContext.Users
            .Include(u => u.UserConfig)
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _queryDbContext.Users
            .Include(u => u.UserConfig)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        _commandDbContext.Users.Update(user);
        await _commandDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserSolution>> GetUserSolutionsAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        return await _queryDbContext.UserSolutions
            .Include(s => s.Problem)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationUser>> SearchAsync(
        string query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalized = query.Trim();

        var q = _queryDbContext.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            var lower = normalized.ToLowerInvariant();

            if (_queryDbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                q = q.Where(u =>
                    (u.UserName != null && u.UserName.ToLower().Contains(lower)) ||
                    (u.Email != null && u.Email.ToLower().Contains(lower)));
            }
            else
            {
                var like = "%" + lower + "%";

                q = q.Where(u =>
                    (u.UserName != null && EF.Functions.ILike(u.UserName, like)) ||
                    (u.Email != null && EF.Functions.ILike(u.Email, like)));
            }
        }

        q = q
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await q.ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<ApplicationUser> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var q = _queryDbContext.Users.AsNoTracking();

        var total = await q.CountAsync(cancellationToken);

        var items = await q
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<(IReadOnlyList<ApplicationUser> Items, int TotalCount)> SearchByUsernamePagedAsync(
        string query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalized = (query).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return (Array.Empty<ApplicationUser>(), 0);

        var usersQuery = _queryDbContext.Users.AsNoTracking();

        var isInMemory = _queryDbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
        var lower = normalized.ToLowerInvariant();
        var like = "%" + lower + "%";

        IQueryable<ApplicationUser> q;

        if (isInMemory)
            q = usersQuery.Where(u => u.UserName != null && u.UserName.ToLower().Contains(lower));
        else
            q = usersQuery.Where(u => u.UserName != null && EF.Functions.ILike(u.UserName, like));

        var total = await q.CountAsync(cancellationToken);

        var items = await q
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<(IReadOnlyList<ApplicationUser> Items, int TotalCount)> SearchByEmailPagedAsync(
        string query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalized = (query).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return (Array.Empty<ApplicationUser>(), 0);

        var usersQuery = _queryDbContext.Users.AsNoTracking();

        var isInMemory = _queryDbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
        var lower = normalized.ToLowerInvariant();
        var like = "%" + lower + "%";

        IQueryable<ApplicationUser> q;

        if (isInMemory)
            q = usersQuery.Where(u => u.Email != null && u.Email.ToLower().Contains(lower));
        else
            q = usersQuery.Where(u => u.Email != null && EF.Functions.ILike(u.Email, like));

        var total = await q.CountAsync(cancellationToken);

        var items = await q
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
