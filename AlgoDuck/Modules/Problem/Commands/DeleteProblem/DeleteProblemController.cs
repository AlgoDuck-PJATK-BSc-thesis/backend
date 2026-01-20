using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.DeleteProblem;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/[controller]")]
public class DeleteProblemController : ControllerBase
{
    private readonly IDeleteProblemService _deleteProblemService;

    public DeleteProblemController(IDeleteProblemService deleteProblemService)
    {
        _deleteProblemService = deleteProblemService;
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteProblemAsync([FromQuery] Guid problemId,
        CancellationToken cancellationToken = default)
    {
        return await _deleteProblemService.DeleteProblemAsync(problemId, cancellationToken).ToActionResultAsync();
    }
}