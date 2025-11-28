using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemsByCategory;

[ApiController]
[Route("api/[controller]")]
public class CategoryProblemsController(
    ICategoryProblemsService categoryProblemsService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllProblemsForCategory([FromQuery] string categoryName)
    {
        return Ok(new StandardApiResponse<ICollection<ProblemDisplayDto>>
        {
            Body = await categoryProblemsService.GetAllProblemsForCategoryAsync(categoryName)
        });
    }
}