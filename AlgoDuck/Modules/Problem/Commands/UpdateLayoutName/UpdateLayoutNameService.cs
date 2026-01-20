using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.UpdateLayoutName;

public interface IUpdateLayoutNameService
{
    public Task<Result<RenameLayoutResultDto, ErrorObject<string>>> RenameLayoutAsync(RenameLayoutRequestDto requestDto,
        CancellationToken cancellationToken = default);
}

public class UpdateLayoutNameService : IUpdateLayoutNameService
{
    private readonly IUpdateLayoutNameRepository _updateLayoutNameRepository;

    public UpdateLayoutNameService(IUpdateLayoutNameRepository updateLayoutNameRepository)
    {
        _updateLayoutNameRepository = updateLayoutNameRepository;
    }

    public async Task<Result<RenameLayoutResultDto, ErrorObject<string>>> RenameLayoutAsync(
        RenameLayoutRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        return await _updateLayoutNameRepository.RenameLayoutAsync(requestDto, cancellationToken);
    }
}
