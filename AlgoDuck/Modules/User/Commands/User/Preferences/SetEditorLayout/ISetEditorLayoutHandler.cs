namespace AlgoDuck.Modules.User.Commands.User.Preferences.SetEditorLayout;

public interface ISetEditorLayoutHandler
{
    Task HandleAsync(Guid userId, SetEditorLayoutDto dto, CancellationToken cancellationToken);
}