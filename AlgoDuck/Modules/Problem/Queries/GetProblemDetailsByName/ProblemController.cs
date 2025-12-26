using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;

[ApiController]
[Route("api/[controller]")]
public class ProblemController(IProblemService problemService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProblemDetailsByIdAsync([FromQuery] Guid problemId)
    {
        return Ok(new StandardApiResponse<ProblemDto>
        {
            Body = await problemService.GetProblemDetailsAsync(problemId)
        });
    }
}