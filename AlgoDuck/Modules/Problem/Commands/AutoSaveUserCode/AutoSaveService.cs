using AlgoDuck.Shared.Http;
using FluentValidation;

namespace AlgoDuck.Modules.Problem.Commands.AutoSaveUserCode;

public interface IAutoSaveService
{
    public Task<Result<AutoSaveResultDto, ErrorObject<string>>> AutoSaveCodeAsync(AutoSaveDto autoSaveDto,
        CancellationToken cancellationToken);
}

public class AutoSaveService : IAutoSaveService
{
    private readonly IAutoSaveRepository _autoSaveRepository;

    public AutoSaveService(IAutoSaveRepository autoSaveRepository)
    {
        _autoSaveRepository = autoSaveRepository;
    }

    public async Task<Result<AutoSaveResultDto, ErrorObject<string>>> AutoSaveCodeAsync(AutoSaveDto autoSaveDto,
        CancellationToken cancellationToken)
    {
        return await _autoSaveRepository.UpsertSolutionSnapshotCodeAsync(autoSaveDto, cancellationToken);
    }
}