using ExecutorService.Executor._ExecutorUtils;
using Microsoft.AspNetCore.Mvc;

namespace ExecutorService.Executor;

[ApiController]
[Route("/api/execute")]
public class ExecutorApiController(ICodeExecutorService codeExecutorService) : ControllerBase
{
    [HttpPost("full")]
    public async Task<IActionResult> ExecuteCode([FromBody] ExecuteRequestDto executeRequest)
    {
        return Ok(await codeExecutorService.FullExecute(executeRequest));
    }
    [HttpPost("dry")]
    public async Task<IActionResult> DryExecuteCode([FromBody] ExecuteRequestDto executeRequest)
    {
        return Ok(await codeExecutorService.DryExecute(executeRequest));
    }
}