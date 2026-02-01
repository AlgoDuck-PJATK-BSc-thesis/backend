using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetAllProblemsForCategory;

[ApiController]
[Route("api/problem/all")]
[Authorize]
public class CategoryProblemsController : ControllerBase
{
    private readonly ICategoryProblemsService _categoryProblemsService;

    public CategoryProblemsController(ICategoryProblemsService categoryProblemsService)
    {
        _categoryProblemsService = categoryProblemsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProblemsForCategory([FromQuery] Guid categoryId, CancellationToken cancellationToken)
    {
        return await User
            .GetUserId()
            .BindAsync(async userId => await _categoryProblemsService.GetAllProblemsForCategoryAsync(categoryId, userId, cancellationToken))
            .ToActionResultAsync();
 
    }
}