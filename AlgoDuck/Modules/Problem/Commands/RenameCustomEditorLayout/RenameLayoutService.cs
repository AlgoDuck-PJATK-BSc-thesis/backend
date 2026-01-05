using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.RenameCustomEditorLayout;

public interface IRenameLayoutService
{
    public Task<Result<RenameLayoutResultDto, ErrorObject<string>>> RenameLayoutAsync(RenameLayoutRequestDto renameLayoutRequestDto, CancellationToken cancellationToken = default);
}

public class RenameLayoutService(
    IRenameLayoutRepository renameLayoutRepository
    ) : IRenameLayoutService
{
    
    public Task<Result<RenameLayoutResultDto, ErrorObject<string>>> RenameLayoutAsync(RenameLayoutRequestDto renameRequest, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
