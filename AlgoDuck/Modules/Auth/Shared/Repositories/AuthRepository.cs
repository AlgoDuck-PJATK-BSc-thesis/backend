using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Auth.Shared.Repositories;

public sealed class AuthRepository : IAuthRepository
{
    private readonly ApplicationCommandDbContext _commandDbContext;

    public AuthRepository(ApplicationCommandDbContext commandDbContext)
    {
        _commandDbContext = commandDbContext;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _commandDbContext.ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<ApplicationUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _commandDbContext.ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<ApplicationUser?> FindByUserNameAsync(string userName, CancellationToken cancellationToken)
    {
        return await _commandDbContext.ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }
}