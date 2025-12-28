using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.AutoSaveUserCode;

public interface IAutoSaveService
{
    public Task<Result<bool, ErrorObject<string>>> AutoSaveCodeAsync(AutoSaveDto autoSaveDto,
        CancellationToken cancellationToken);
}

public class AutoSaveService(
    IAutoSaveRepository autoSaveRepository
) : IAutoSaveService
{
    public async Task<Result<bool, ErrorObject<string>>> AutoSaveCodeAsync(AutoSaveDto autoSaveDto,
        CancellationToken cancellationToken)
    {
        return await autoSaveRepository.UpsertSolutionSnapshotCodeAsync(autoSaveDto, cancellationToken);
    }
}