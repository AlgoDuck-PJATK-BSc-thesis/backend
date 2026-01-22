using System.ComponentModel.DataAnnotations;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Shared.Http;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;

[ApiController]
[Authorize]
[Route("/api/executor/[controller]")]
[EnableRateLimiting("CodeExecution")]
public class DryRunController : ControllerBase
{
    
    private readonly IExecutorDryRunService _executorService;
    private readonly IValidator<DryRunExecuteRequest> _validator;

    public DryRunController(IExecutorDryRunService executorService, IValidator<DryRunExecuteRequest> validator)
    {
        _executorService = executorService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> ExecuteCode([FromBody] DryRunExecuteRequest executeRequest, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(executeRequest, cancellationToken);
        if  (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        return await User.GetUserId().BindAsync(async userId =>
        {
            executeRequest.UserId = userId;
            return await _executorService.DryRunUserCodeAsync(executeRequest, cancellationToken);
        }).ToActionResultAsync();
        
    }
}

public class DryRunExecuteRequest
{
    private const int MaxCodeLengthBytes = 128 * 1024;
    [MaxLength(MaxCodeLengthBytes)]
    public required string CodeB64 { get; set; }
    internal Guid UserId { get; set; } 
}