using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetAllProblemCategories;

public interface IProblemCategoriesService
{
    public Task<Result<IEnumerable<CategoryDto>, ErrorObject<string>>> GetAllAsync(CancellationToken cancellationToken = default);
}

public class ProblemCategoriesService : IProblemCategoriesService
{
    private readonly IProblemCategoriesRepository _repository;

    public ProblemCategoriesService(IProblemCategoriesRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<CategoryDto>, ErrorObject<string>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}