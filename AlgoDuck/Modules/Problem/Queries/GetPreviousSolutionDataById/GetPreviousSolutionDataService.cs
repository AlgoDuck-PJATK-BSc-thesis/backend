using AlgoDuck.Shared.Result;

namespace AlgoDuck.Modules.Problem.Queries.GetPreviousSolutionDataById;

public interface IGetPreviousSolutionDataService
{
    public Task<Result<SolutionData, ErrorUnion<NotFoundError<string>, InternalError<string>>>> GetPreviousSolutionDataAsync(PreviousSolutionRequestDto requestDto, CancellationToken cancellationToken = default);
}

public class GetPreviousSolutionDataService : IGetPreviousSolutionDataService
{
    private readonly IGetPreviousSolutionDataRepository _getPreviousSolutionDataRepository;

    public GetPreviousSolutionDataService(IGetPreviousSolutionDataRepository getPreviousSolutionDataRepository)
    {
        _getPreviousSolutionDataRepository = getPreviousSolutionDataRepository;
    }

    public Task<Result<SolutionData, ErrorUnion<NotFoundError<string>, InternalError<string>>>> GetPreviousSolutionDataAsync(PreviousSolutionRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        return _getPreviousSolutionDataRepository.GetPreviousSolutionDataAsync(requestDto, cancellationToken);
    }
}