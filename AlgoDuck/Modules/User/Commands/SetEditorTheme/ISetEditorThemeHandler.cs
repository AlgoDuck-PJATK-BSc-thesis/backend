namespace AlgoDuck.Modules.User.Commands.SetEditorTheme;

public interface ISetEditorThemeHandler
{
    Task HandleAsync(Guid userId, SetEditorThemeDto dto, CancellationToken cancellationToken);
}