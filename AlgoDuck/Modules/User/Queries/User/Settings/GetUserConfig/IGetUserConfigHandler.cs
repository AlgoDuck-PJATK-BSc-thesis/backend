namespace AlgoDuck.Modules.User.Queries.User.Settings.GetUserConfig;

public interface IGetUserConfigHandler
{
    Task<UserConfigDto> HandleAsync(GetUserConfigRequestDto query, CancellationToken cancellationToken);
}