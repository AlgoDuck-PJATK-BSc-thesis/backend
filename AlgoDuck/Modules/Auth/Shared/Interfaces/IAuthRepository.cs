using AlgoDuck.Models;

namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface IAuthRepository
{
    Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<ApplicationUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<ApplicationUser?> FindByUserNameAsync(string userName, CancellationToken cancellationToken);
}