using AlgoDuck.Modules.Problem.DTOs.ProblemDtos;
using AlgoDuck.Modules.Problem.Repositories;

namespace AlgoDuck.Modules.Problem.Services;

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