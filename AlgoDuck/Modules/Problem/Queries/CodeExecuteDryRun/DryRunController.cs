using AlgoDuck.Shared.Http;
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
        return Ok(new StandardApiResponse<ExecuteResponse>
        {
            Body = await executorService.DryRunUserCodeAsync(executeRequest)
        });
    }
}