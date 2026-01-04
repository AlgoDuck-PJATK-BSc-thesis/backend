namespace AlgoDuck.Modules.User.Queries.User.Settings.GetTwoFactorEnabled;

public interface IGetTwoFactorEnabledHandler
{
    Task<TwoFactorStatusDto> HandleAsync(Guid userId, CancellationToken cancellationToken);
}