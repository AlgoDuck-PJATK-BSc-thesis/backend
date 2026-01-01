using AlgoDuck.Modules.Problem.Shared.Repositories;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;

public interface IProblemService
{
    public Task<Result<ProblemDto, ErrorObject<string>>> GetProblemDetailsAsync(Guid problemId);
}

public class ProblemService(
    ISharedProblemRepository problemRepository
    ) : IProblemService
{
    public async Task<Result<ProblemDto, ErrorObject<string>>> GetProblemDetailsAsync(Guid problemId)
    {
        return await problemRepository.GetProblemDetailsAsync(problemId);
    }
}