namespace AlgoDuck.Modules.User.Shared.Interfaces;

public interface IDefaultDuckService
{
    Task EnsureAlgoduckOwnedAndSelectedAsync(Guid userId, CancellationToken cancellationToken);
}