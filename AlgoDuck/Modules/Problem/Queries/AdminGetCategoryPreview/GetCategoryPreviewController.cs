using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.AdminGetCategoryPreview;

[Route("api/problem/category/preview")]
[ApiController]
[Authorize(Roles = "admin")]
public class GetCategoryPreviewController : ControllerBase
{
    private readonly IGetCategoryPreviewService _getCategoryPreviewService;

    public GetCategoryPreviewController(IGetCategoryPreviewService getCategoryPreviewService)
    {
        _getCategoryPreviewService = getCategoryPreviewService;
    }

    public async Task<IActionResult> GetCategoryPreviewAsync([FromQuery] Guid categoryId,
        CancellationToken cancellationToken)
    {
        return await _getCategoryPreviewService.GetCategoryPreviewAsync(categoryId, cancellationToken)
            .ToActionResultAsync();
    }
}

public interface IGetCategoryPreviewService
{
    public Task<Result<CategoryPreviewDto, ErrorObject<string>>> GetCategoryPreviewAsync(Guid categoryId, CancellationToken cancellationToken = default);
}

public class GetCategoryPreviewService : IGetCategoryPreviewService
{
    private readonly IGetCategoryPreviewRepository _categoryPreviewRepository;

    public GetCategoryPreviewService(IGetCategoryPreviewRepository categoryPreviewRepository)
    {
        _categoryPreviewRepository = categoryPreviewRepository;
    }

    public async Task<Result<CategoryPreviewDto, ErrorObject<string>>> GetCategoryPreviewAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _categoryPreviewRepository.GetCategoryPreviewAsync(categoryId, cancellationToken);
    }
}

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

public sealed class CategoryPreviewDto
{
    public required Guid CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public required int ProblemCount { get; init; }
}