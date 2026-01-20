using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.LoadLastUserAutoSaveForProblem;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LoadAutoSaveController : ControllerBase
{
    
    private readonly ILoadAutoSaveService _loadAutoSaveService;

    public LoadAutoSaveController(ILoadAutoSaveService loadAutoSaveService)
    {
        this._loadAutoSaveService = loadAutoSaveService;
    }

    [HttpGet]
    public async Task<IActionResult> LoadProblemByIdAsync([FromQuery] Guid problemId, CancellationToken cancellationToken)
    {
        return await User.GetUserId()
            .BindAsync(async userId => await _loadAutoSaveService.LoadAutoSaveController(new AutoSaveRequestDto
            {
                ProblemId = problemId,
                UserId = userId
            }, cancellationToken))
            .ToActionResultAsync();
        
    }
}