using System.Text.Json;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetAllProblemsForCategory;

public interface ICategoryProblemsService
{
    public Task<Result<CategoryDto, ErrorObject<string>>> GetAllProblemsForCategoryAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default);
}

public class CategoryProblemsService : ICategoryProblemsService
{
    private readonly ICategoryProblemsRepository _categoryProblemsRepository;

    public CategoryProblemsService(ICategoryProblemsRepository categoryProblemsRepository)
    {
        _categoryProblemsRepository = categoryProblemsRepository;
    }

    public async Task<Result<CategoryDto, ErrorObject<string>>> GetAllProblemsForCategoryAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _categoryProblemsRepository.GetAllProblemsForCategoryAsync(categoryId, userId,
            cancellationToken);
    }
}