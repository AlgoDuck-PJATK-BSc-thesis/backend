namespace AlgoDuck.Modules.User.Commands.User.Preferences.SetEditorTheme;

public interface ISetEditorThemeHandler
{
    Task HandleAsync(Guid userId, SetEditorThemeDto dto, CancellationToken cancellationToken);
}