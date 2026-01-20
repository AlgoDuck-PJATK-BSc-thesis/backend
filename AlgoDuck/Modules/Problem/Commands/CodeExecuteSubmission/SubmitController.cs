using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Modules.Problem.Shared.Types;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;

[ApiController]
[Route("/api/executor/[controller]")]
[Authorize]
[EnableRateLimiting("CodeExecution")]
public class SubmitController : ControllerBase
{
    private readonly IExecutorSubmitService _executorService;

    public SubmitController(IExecutorSubmitService executorService)
    {
        _executorService = executorService;
    }

    [HttpPost]
    public async Task<IActionResult> ExecuteCode([FromBody] SubmitExecuteRequest executeRequest, CancellationToken cancellationToken)
    {
        return await User.GetUserId().BindAsync(async userId =>
        {
            executeRequest.UserId = userId;
            return await _executorService.SubmitUserCodeRabbitAsync(executeRequest, cancellationToken);
        }).ToActionResultAsync();
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

public class ExecutionQueueJobDataPublic
{
    public required Guid JobId { get; set; }
    public required Guid UserId { get; set; }
    public Guid? ProblemId { get; set; }
    public List<SubmitExecuteResponse> CachedResponses { get; set; } = [];
}

public enum JobType : byte
{
    DryRun, Testing
}

public class SubmitExecuteRequest
{
    private const int MaxCodeLengthBytes = 128 * 1024;
    internal Guid JobId { get; set; }
    internal Guid UserId { get; set; }
    public required Guid ProblemId { get; set; }
    [MaxLength(MaxCodeLengthBytes)]
    public required string CodeB64 { get; set; }
}