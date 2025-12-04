namespace AlgoDuck.Modules.User.Queries.GetUserConfig;

public interface IGetUserConfigHandler
{
    Task<UserConfigDto> HandleAsync(GetUserConfigQuery query, CancellationToken cancellationToken);
}