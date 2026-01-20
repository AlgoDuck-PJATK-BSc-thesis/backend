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

public sealed class CategoryPreviewDto
{
    public required Guid CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public required int ProblemCount { get; init; }
}