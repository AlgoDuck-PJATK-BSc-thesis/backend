using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProblemController : ControllerBase
{
    private readonly IProblemService _problemService;

    public ProblemController(IProblemService problemService)
    {
        _problemService = problemService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProblemDetailsByIdAsync([FromQuery] Guid problemId, CancellationToken cancellationToken)
    {
        return await _problemService.GetProblemDetailsAsync(problemId, cancellationToken).ToActionResultAsync();
    }
}