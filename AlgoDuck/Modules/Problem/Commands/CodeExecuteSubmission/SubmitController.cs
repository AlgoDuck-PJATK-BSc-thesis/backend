using System.Security.Claims;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;

[ApiController]
[Route("/api/executor/[controller]")]
[Authorize/*("user,admin")*/]
public class SubmitController(IExecutorSubmitService executorService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ExecuteCode([FromBody] SubmitExecuteRequest executeRequest, CancellationToken cancellationToken)
    {
        var userIdResult = User.GetUserId();
        if (userIdResult.IsErr)
            return userIdResult.ToActionResult();

        executeRequest.UserId = userIdResult.AsT0;

        var res = await executorService.SubmitUserCodeRabbitAsync(executeRequest, cancellationToken);
        return res.ToActionResult();
    }
}

public class ExecutionEnqueueingResultDto
{
    public required Guid JobId { get; set; }
    public required Guid UserId { get; set; }
}

public class ExecutionQueueJobData
{
    public required Guid JobId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid SigningKey { get; set; }
    public Guid? ProblemId { get; set; }
    public required string UserCodeB64 { get; set; } 
    public required JobType JobType { get; set; }
    public List<SubmitExecuteResponse> CachedResponses { get; set; } = [];

}

public enum JobType : byte
{
    DryRun, Testing
}

public class SubmitExecuteRequest
{
    internal Guid JobId { get; set; }
    internal Guid UserId { get; set; }
    public required Guid ProblemId { get; set; }
    public required string CodeB64 { get; set; }
}
