using AlgoDuck.DAL;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetAllProblemCategories;

public interface IProblemCategoriesRepository
{
    public Task<IEnumerable<CategoryDto>> GetAllAsync();
}

public class ProblemCategoriesRepository(
    ApplicationQueryDbContext dbContext
    ) : IProblemCategoriesRepository
{
    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        return await dbContext.Categories.Select(c => new CategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
        }).ToListAsync();
    }
}