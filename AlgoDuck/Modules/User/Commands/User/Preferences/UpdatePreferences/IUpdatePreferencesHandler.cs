namespace AlgoDuck.Modules.User.Commands.User.Preferences.UpdatePreferences;

public interface IUpdatePreferencesHandler
{
    Task HandleAsync(Guid userId, UpdatePreferencesDto dto, CancellationToken cancellationToken);
}