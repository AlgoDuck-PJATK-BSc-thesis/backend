using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetAllProblemCategories;

public interface IProblemCategoriesRepository
{
    public Task<Result<IEnumerable<CategoryDto>, ErrorObject<string>>> GetAllAsync(CancellationToken cancellationToken = default);
    
}

public class ProblemCategoriesRepository : IProblemCategoriesRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public ProblemCategoriesRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<CategoryDto>, ErrorObject<string>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Result<IEnumerable<CategoryDto>, ErrorObject<string>>.Ok(await _dbContext.Categories.Select(c =>
            new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
            }).ToListAsync(cancellationToken: cancellationToken));
    }
}