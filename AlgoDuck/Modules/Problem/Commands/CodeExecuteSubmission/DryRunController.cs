using AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;
using AlgoDuckShared.Executor.SharedTypes;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;

[ApiController]
[Route("/api/executor/[controller]")]
public class DryRunController(IExecutorDryRunService executorService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SubmitCode([FromBody] DryExecuteRequest executeRequest)
    {
        return Ok(await executorService.DryRunUserCodeAsync(executeRequest));
    }
}