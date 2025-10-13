using AlgoDuck.Modules.Problem.Queries.GetProblemDetailsById.ProblemDtos;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemDetailsById;

public interface IProblemService
{
    public Task<ProblemDto> GetProblemDetailsAsync(Guid problemId);
}

public class ProblemService(IProblemRepository problemRepository) : IProblemService
{
    private readonly IProblemRepository _problemRepository = problemRepository;

    public async Task<ProblemDto> GetProblemDetailsAsync(Guid problemId)
    {
        return await _problemRepository.GetProblemDetailsAsync(problemId);
    }
}