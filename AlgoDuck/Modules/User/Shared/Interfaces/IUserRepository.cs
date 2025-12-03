using AlgoDuck.Models;

namespace AlgoDuck.Modules.User.Shared.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetByNameAsync(string userName, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken);
}