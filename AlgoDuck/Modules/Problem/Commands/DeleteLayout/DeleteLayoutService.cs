using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.DeleteLayout;

public interface IDeleteLayoutService
{
    public Task<Result<DeleteLayoutResult, ErrorObject<string>>> DeleteLayoutAsync(DeleteLayoutRequest request, CancellationToken cancellationToken = default);
}

public class DeleteLayoutService : IDeleteLayoutService
{
    private readonly IDeleteLayoutRepository _deleteLayoutRepository;

    public DeleteLayoutService(IDeleteLayoutRepository deleteLayoutRepository)
    {
        _deleteLayoutRepository = deleteLayoutRepository;
    }

    public async Task<Result<DeleteLayoutResult, ErrorObject<string>>> DeleteLayoutAsync(DeleteLayoutRequest request, CancellationToken cancellationToken = default)
    {
        return await _deleteLayoutRepository.DeleteLayoutAsync(request, cancellationToken);
    }
}
