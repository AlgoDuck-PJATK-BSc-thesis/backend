namespace AlgoDuck.Modules.Problem.Queries.GetAllProblemCategories;

public interface IProblemCategoriesService
{
    public Task<IEnumerable<CategoryDto>> GetAllAsync();
}

public class ProblemCategoriesService(
    IProblemCategoriesRepository repository
    ) : IProblemCategoriesService
{
    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        return await repository.GetAllAsync();
        throw new NotImplementedException();
    }
}