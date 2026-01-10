using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetAllProblemsForCategory;

[ApiController]
[Route("api/problem/all")]
[Authorize]
public class CategoryProblemsController(
    ICategoryProblemsService categoryProblemsService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllProblemsForCategory([FromQuery] Guid categoryId, CancellationToken cancellationToken)
    {
        return await categoryProblemsService
            .GetAllProblemsForCategoryAsync(categoryId, cancellationToken)
            .ToActionResultAsync();
    }
}