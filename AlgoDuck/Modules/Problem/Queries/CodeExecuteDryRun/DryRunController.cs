using System.ComponentModel.DataAnnotations;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
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
    private const int MaxCodeLengthBytes = 128 * 1024;
    [MaxLength(MaxCodeLengthBytes)]
    public required string CodeB64 { get; set; }
    internal Guid UserId { get; set; } 
}