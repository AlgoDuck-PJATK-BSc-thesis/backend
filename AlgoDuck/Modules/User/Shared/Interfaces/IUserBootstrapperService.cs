namespace AlgoDuck.Modules.User.Shared.Interfaces;

public interface IUserBootstrapperService
{
    Task EnsureUserInitializedAsync(Guid userId, CancellationToken cancellationToken);
}