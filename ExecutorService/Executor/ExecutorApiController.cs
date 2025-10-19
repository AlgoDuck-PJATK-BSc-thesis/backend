using AlgoDuckShared.Executor.SharedTypes;
using Microsoft.AspNetCore.Mvc;

namespace ExecutorService.Executor;

[ApiController]
[Route("/api/execute")]
public class ExecutorApiController(ICodeExecutorService codeExecutorService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Execute([FromBody] ExecuteRequest request)
    {
        return Ok(await codeExecutorService.ExecuteAgnostic(request));
    }
}