using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.LoadLastUserAutoSaveForProblem;

public interface ILoadAutoSaveService
{
    public Task<Result<AutoSaveResponseDto?, ErrorObject<string>>> LoadAutoSaveController(AutoSaveRequestDto request, CancellationToken cancellationToken = default);
    
}

public class LoadAutoSaveService(
    ILoadAutoSaveRepository autoSaveRepository
) : ILoadAutoSaveService
{
    public async Task<Result<AutoSaveResponseDto?, ErrorObject<string>>> LoadAutoSaveController(AutoSaveRequestDto request, CancellationToken cancellationToken = default)
    {
        return await autoSaveRepository.TryGetLastAutoSaveAsync(request, cancellationToken);
    }
}