using AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;
using AlgoDuckShared.Executor.SharedTypes;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;

[ApiController]
[Route("/api/[controller]")]
public class ExecuteSubmitController(IExecutorSubmitService executorService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SubmitCode([FromBody] SubmitExecuteRequest executeRequest)
    {
        return Ok(await executorService.SubmitUserCode(executeRequest));
    }
}