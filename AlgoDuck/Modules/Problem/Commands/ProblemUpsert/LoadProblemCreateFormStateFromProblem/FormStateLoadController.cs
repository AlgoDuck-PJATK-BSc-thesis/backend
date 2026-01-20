using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.LoadProblemCreateFormStateFromProblem;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/[controller]")]
public class FormStateLoadController : ControllerBase
{
    private readonly IFormStateLoadService _formStateLoadService;

    public FormStateLoadController(IFormStateLoadService formStateLoadService)
    {
        _formStateLoadService = formStateLoadService;
    }

    [HttpGet]
    public async Task<IActionResult> LoadFormStateAsync([FromQuery] Guid problemId, CancellationToken cancellationToken)
    {
        return await _formStateLoadService.LoadFormStateAsync(problemId, cancellationToken).ToActionResultAsync();
    }
}