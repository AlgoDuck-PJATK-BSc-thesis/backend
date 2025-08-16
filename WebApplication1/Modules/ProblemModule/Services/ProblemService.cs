using WebApplication1.Modules.ProblemModule.DTOs.ProblemDtos;
using WebApplication1.Modules.ProblemModule.Repositories;

namespace WebApplication1.Modules.ProblemModule.Services;

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