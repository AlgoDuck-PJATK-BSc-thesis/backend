using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        return await User.GetUserId()
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