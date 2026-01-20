using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.AdminGetCategoryPreview;

public interface IGetCategoryPreviewRepository
{
    public Task<Result<CategoryPreviewDto, ErrorObject<string>>> GetCategoryPreviewAsync(Guid categoryId, CancellationToken cancellationToken = default);
}

public class GetCategoryPreviewRepository : IGetCategoryPreviewRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public GetCategoryPreviewRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CategoryPreviewDto, ErrorObject<string>>> GetCategoryPreviewAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .Include(c => c.Problems)
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId, cancellationToken: cancellationToken);
        if (category == null)
            return Result<CategoryPreviewDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Category: {categoryId} not found"));
        
        return Result<CategoryPreviewDto, ErrorObject<string>>.Ok(new CategoryPreviewDto()
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            ProblemCount = category.Problems.Count
        });
    }
}
