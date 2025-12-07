namespace AlgoDuck.Modules.Auth.Commands.Logout;

public interface ILogoutHandler
{
    Task HandleAsync(LogoutDto dto, Guid? currentUserId, Guid? currentSessionId, CancellationToken cancellationToken);
}