using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemsByCategory;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryProblemsController(
    ICategoryProblemsService categoryProblemsService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllProblemsForCategory([FromQuery] string categoryName)
    {
        return await categoryProblemsService
            .GetAllProblemsForCategoryAsync(categoryName)
            .ToActionResultAsync();
    }
}