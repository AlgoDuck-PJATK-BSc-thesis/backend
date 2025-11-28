using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetAllProblemCategories;

[ApiController]
[Route("api/[controller]")]
[Authorize/*("user,admin")*/]
public class ProblemCategoriesController(
    IProblemCategoriesService service
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllCategoriesAsync()
    {
        return Ok(new StandardApiResponse<IEnumerable<CategoryDto>>()
        {
            Body = await service.GetAllAsync()
        });
    } 
}