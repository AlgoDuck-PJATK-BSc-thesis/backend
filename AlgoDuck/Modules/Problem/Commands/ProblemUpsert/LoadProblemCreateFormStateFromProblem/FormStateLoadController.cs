using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.LoadProblemCreateFormStateFromProblem;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/[controller]")]
public class FormStateLoadController(
    IFormStateLoadService formStateLoadService
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> LoadFormStateAsync([FromQuery] Guid problemId, CancellationToken cancellationToken)
    {
        return await formStateLoadService.LoadFormStateAsync(problemId, cancellationToken).ToActionResultAsync();
    }
}