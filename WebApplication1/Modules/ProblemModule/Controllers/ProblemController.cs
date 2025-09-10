using Microsoft.AspNetCore.Mvc;
using WebApplication1.Modules.ProblemModule.Services;

namespace WebApplication1.Modules.ProblemModule.Controllers;

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