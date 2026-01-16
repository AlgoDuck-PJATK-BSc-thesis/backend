using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetAllProblemsForCategory;

public interface ICategoryProblemsService
{
    public Task<Result<ICollection<ProblemDisplayDto>, ErrorObject<string>>> GetAllProblemsForCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
}

public class CategoryProblemsService : ICategoryProblemsService
{
    private readonly ICategoryProblemsRepository _categoryProblemsRepository;

    public CategoryProblemsService(ICategoryProblemsRepository categoryProblemsRepository)
    {
        _categoryProblemsRepository = categoryProblemsRepository;
    }

    public async Task<Result<ICollection<ProblemDisplayDto>, ErrorObject<string>>> GetAllProblemsForCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _categoryProblemsRepository.GetAllProblemsForCategoryAsync(categoryId, cancellationToken);
    }
}