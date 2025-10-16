using AlgoDuckShared.Executor.SharedTypes;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;

[ApiController]
[Route("/api/[controller]")]
public class ExecuteDryController(IExecutorDryService executorService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ExecuteCode([FromBody] DryExecuteRequest executeRequest)
    {
        return Ok(await executorService.DryRunUserCode(executeRequest));
    }
}