using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuckShared.Executor.SharedTypes;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;

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