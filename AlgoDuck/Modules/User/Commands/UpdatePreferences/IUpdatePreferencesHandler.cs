namespace AlgoDuck.Modules.User.Commands.UpdatePreferences;

public interface IUpdatePreferencesHandler
{
    Task HandleAsync(Guid userId, UpdatePreferencesDto dto, CancellationToken cancellationToken);
}