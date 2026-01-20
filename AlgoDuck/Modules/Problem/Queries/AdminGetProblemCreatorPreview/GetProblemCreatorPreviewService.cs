using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.AdminGetProblemCreatorPreview;

public interface IGetProblemCreatorPreviewService
{
    public Task<Result<ProblemCreator, ErrorObject<string>>> GetProblemCreatorAsync(Guid userId,
        CancellationToken cancellationToken = default);
}

public class GetProblemCreatorPreviewService : IGetProblemCreatorPreviewService
{
    private readonly IGetProblemCreatorPreviewRepository _getProblemCreatorPreviewRepository;

    public GetProblemCreatorPreviewService(IGetProblemCreatorPreviewRepository getProblemCreatorPreviewRepository)
    {
        _getProblemCreatorPreviewRepository = getProblemCreatorPreviewRepository;
    }

    public async Task<Result<ProblemCreator, ErrorObject<string>>> GetProblemCreatorAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _getProblemCreatorPreviewRepository.GetProblemCreatorAsync(userId, cancellationToken);
    }
}