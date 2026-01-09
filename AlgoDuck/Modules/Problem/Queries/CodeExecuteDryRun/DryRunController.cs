using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;

[ApiController]
[Authorize]
[Route("/api/executor/[controller]")]
public class DryRunController(IExecutorDryRunService executorService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ExecuteCode([FromBody] DryRunExecuteRequest executeRequest)
    {
        var userIdResult = User.GetUserId();
        if (userIdResult.IsErr)
            return userIdResult.ToActionResult();

        executeRequest.UserId = userIdResult.AsT0;
        var res = await executorService.DryRunUserCodeAsync(executeRequest);
        return res.ToActionResult();
    }
}

public class DryRunExecuteRequest
{
    public required string CodeB64 { get; set; }
    internal Guid UserId { get; set; } 
}

public class SubmitExecuteRequest
{
    public required Guid ProblemId { get; set; }
    public required string CodeB64 { get; set; }
}