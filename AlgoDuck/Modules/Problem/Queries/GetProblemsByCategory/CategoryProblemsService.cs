namespace AlgoDuck.Modules.Problem.Queries.GetProblemsByCategory;

public interface ICategoryProblemsService
{
    public Task<ICollection<ProblemDisplayDto>> GetAllProblemsForCategoryAsync(string categoryName);
}

public class CategoryProblemsService(
    ICategoryProblemsRepository categoryProblemsRepository
    ) : ICategoryProblemsService
{
    public async Task<ICollection<ProblemDisplayDto>> GetAllProblemsForCategoryAsync(string categoryName)
    {
        return await categoryProblemsRepository.GetAllProblemsForCategoryAsync(categoryName);
        throw new NotImplementedException();
    }
}