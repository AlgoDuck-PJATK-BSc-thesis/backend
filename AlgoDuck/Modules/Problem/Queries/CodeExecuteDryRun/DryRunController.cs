using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;

[ApiController]
[Route("/api/executor/[controller]")]
public class DryRunController(IExecutorDryRunService executorService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ExecuteCode([FromBody] DryRunExecuteRequest executeRequest)
    {
        return Ok(new StandardApiResponse<SubmitExecuteResponse>
        {
            Body = await executorService.DryRunUserCodeAsync(executeRequest)
        });
    }
}

public class DryRunExecuteRequest
{
    public required string CodeB64 { get; set; }
}

public class SubmitExecuteRequest
{
    public required Guid ProblemId { get; set; }
    public required string CodeB64 { get; set; }
}