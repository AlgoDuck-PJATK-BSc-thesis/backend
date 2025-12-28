using AlgoDuck.Shared.Http;
using OneOf.Types;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemsByCategory;

public interface ICategoryProblemsService
{
    public Task<Result<ICollection<ProblemDisplayDto>, ErrorObject<string>>> GetAllProblemsForCategoryAsync(string categoryName);
}

public class CategoryProblemsService(
    ICategoryProblemsRepository categoryProblemsRepository
) : ICategoryProblemsService
{
    public async Task<Result<ICollection<ProblemDisplayDto>, ErrorObject<string>>> GetAllProblemsForCategoryAsync(string categoryName)
    {
        return await categoryProblemsRepository.GetAllProblemsForCategoryAsync(categoryName);
    }
}