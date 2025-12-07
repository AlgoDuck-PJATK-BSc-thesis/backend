namespace AlgoDuck.Modules.User.Queries.GetUserConfig;

public interface IGetUserConfigHandler
{
    Task<UserConfigDto> HandleAsync(GetUserConfigRequestDto query, CancellationToken cancellationToken);
}