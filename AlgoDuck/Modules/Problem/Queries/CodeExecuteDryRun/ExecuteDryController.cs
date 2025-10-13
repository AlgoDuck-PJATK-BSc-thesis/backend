using AlgoDuckShared.Executor.SharedTypes;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;

[ApiController]
[Route("/api/[controller]")]
internal class ExecuteDryController(IExecutorDryService executorService) : ControllerBase
{
    [HttpPost]
    internal async Task<IActionResult> ExecuteCode([FromBody] DryExecuteRequest executeRequest)
    {
        return Ok(await executorService.DryRunUserCode(executeRequest));
    }
}