using AlgoDuck.Shared.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetPreviousSolutionDataById;

[ApiController]
[Authorize]
[Route("api/problem/solution")]
public class PreviousSolutionDataController : ControllerBase
{
    
    private readonly IGetPreviousSolutionDataService _getPreviousSolutionDataService;

    public PreviousSolutionDataController(IGetPreviousSolutionDataService getPreviousSolutionDataService)
    {
        _getPreviousSolutionDataService = getPreviousSolutionDataService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPreviousSolutionDataAsync([FromQuery] Guid solutionId,
        CancellationToken cancellationToken = default)
    {
        return await User.UserIdToResult()
            .BindAsync(async userId =>
                await _getPreviousSolutionDataService.GetPreviousSolutionDataAsync(new PreviousSolutionRequestDto
                {
                    SolutionId = solutionId,
                    UserId = userId,
                }, cancellationToken)).ToActionResultAsync();
    }
}



public class PreviousSolutionRequestDto
{
    public required Guid SolutionId { get; set; }
    internal Guid UserId { get; set; }
}

public class SolutionData
{
    public required Guid SolutionId { get; set; }
    public required string CodeB64 { get; set; }
}