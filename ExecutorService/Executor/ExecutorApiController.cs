using Microsoft.AspNetCore.Mvc;

namespace ExecutorService.Executor;

[ApiController]
[Route("/api/execute")]
public class ExecutorApiController(ICodeExecutorService codeExecutorService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Execute([FromBody] ExecutionRequest request)
    {
        return Ok(await codeExecutorService.Execute(request));
    }
}

public class ExecutionRequest
{
    public required Dictionary<string, string> JavaFiles { get; set; }
}
