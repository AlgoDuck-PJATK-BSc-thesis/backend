using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.LoadLastUserAutoSaveForProblem;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LoadAutoSaveController(
    ILoadAutoSaveService loadProblemService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> LoadProblemByIdAsync([FromQuery] Guid problemId, CancellationToken cancellationToken)
    {
        var userIdResult = User.GetUserId();
        if (userIdResult.IsErr)
            return userIdResult.ToActionResult();
        
        var res = await loadProblemService.LoadAutoSaveController(new AutoSaveRequestDto
        {
            ProblemId = problemId,
            UserId = userIdResult.AsT0
        }, cancellationToken);

        return res.ToActionResult();
    }
}