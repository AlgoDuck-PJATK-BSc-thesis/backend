namespace AlgoDuck.Modules.User.Queries.GetTwoFactorEnabled;

public interface IGetTwoFactorEnabledHandler
{
    Task<TwoFactorStatusDto> HandleAsync(Guid userId, CancellationToken cancellationToken);
}