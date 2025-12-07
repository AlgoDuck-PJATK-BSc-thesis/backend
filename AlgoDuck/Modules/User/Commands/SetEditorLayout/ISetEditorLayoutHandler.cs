namespace AlgoDuck.Modules.User.Commands.SetEditorLayout;

public interface ISetEditorLayoutHandler
{
    Task HandleAsync(Guid userId, SetEditorLayoutDto dto, CancellationToken cancellationToken);
}