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
}