using AlgoDuckShared.Executor.SharedTypes;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;

[ApiController]
[Route("/api/executor/[controller]")]
public class SubmitController(IExecutorSubmitService executorService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ExecuteCode([FromBody] SubmitExecuteRequest executeRequest)
    {
        return Ok(await executorService.SubmitUserCodeAsync(executeRequest));
    }
}