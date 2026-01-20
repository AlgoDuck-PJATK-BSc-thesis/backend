using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.LoadLastUserAutoSaveForProblem;

public interface ILoadAutoSaveService
{
    public Task<Result<AutoSaveResponseDto?, ErrorObject<string>>> LoadAutoSaveController(AutoSaveRequestDto request,
        CancellationToken cancellationToken = default);
}

public class LoadAutoSaveService : ILoadAutoSaveService
{
    private readonly ILoadAutoSaveRepository _autoSaveRepository;

    public LoadAutoSaveService(ILoadAutoSaveRepository autoSaveRepository)
    {
        _autoSaveRepository = autoSaveRepository;
    }

    public async Task<Result<AutoSaveResponseDto?, ErrorObject<string>>> LoadAutoSaveController(
        AutoSaveRequestDto request, CancellationToken cancellationToken = default)
    {
        return await _autoSaveRepository.TryGetLastAutoSaveAsync(request, cancellationToken);
    }
}