using AlgoDuck.Modules.Problem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ProblemController(IProblemService problemService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProblem([FromQuery] Guid id)
    {
        return Ok(await problemService.GetProblemDetailsAsync(id));
    }
}