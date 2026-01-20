using AlgoDuck.Modules.Problem.Shared.Repositories;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;

public interface IProblemService
{
    public Task<Result<ProblemDto, ErrorObject<string>>> GetProblemDetailsAsync(Guid problemId, CancellationToken cancellationToken = default);
}

public class ProblemService : IProblemService
{
    private readonly ISharedProblemRepository _problemRepository;

    public ProblemService(ISharedProblemRepository problemRepository)
    {
        _problemRepository = problemRepository;
    }

    public async Task<Result<ProblemDto, ErrorObject<string>>> GetProblemDetailsAsync(Guid problemId, CancellationToken cancellationToken = default)
    {
        return await _problemRepository.GetProblemDetailsAsync(problemId, cancellationToken);
    }
}