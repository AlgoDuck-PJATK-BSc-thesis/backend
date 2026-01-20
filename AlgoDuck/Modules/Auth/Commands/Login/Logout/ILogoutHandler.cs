namespace AlgoDuck.Modules.Auth.Commands.Login.Logout;

public interface ILogoutHandler
{
    Task HandleAsync(LogoutDto dto, Guid? currentUserId, Guid? currentSessionId, CancellationToken cancellationToken);
}