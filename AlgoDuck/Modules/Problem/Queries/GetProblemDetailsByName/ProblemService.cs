namespace AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;

public interface IProblemService
{
    public Task<ProblemDto> GetProblemDetailsAsync(Guid problemId);
}

public class ProblemService(
    IProblemRepository problemRepository
    ) : IProblemService
{
    public async Task<ProblemDto> GetProblemDetailsAsync(Guid problemId)
    {
        return await problemRepository.GetProblemDetailsAsync(problemId);
    }
}