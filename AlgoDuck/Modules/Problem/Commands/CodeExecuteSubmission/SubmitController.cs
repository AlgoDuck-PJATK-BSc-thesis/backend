using System.Security.Claims;
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
    public async Task<IActionResult> ExecuteCode([FromBody] SubmitExecuteRequest executeRequest)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                     ?? throw new UserNotFoundException();
        Console.WriteLine(DateTime.UtcNow);
        return Ok(new StandardApiResponse<ExecutionTaskRegistrationDto>
        {
            Body = new  ExecutionTaskRegistrationDto
            {
                JobId = await executorService.SubmitUserCodeRabbit(executeRequest, Guid.Parse(userId)),
            }
        });
    }
}

public class ExecutionTaskRegistrationDto
{
    public required Guid JobId { get; set; }
}


public class SubmitExecuteRequest
{
    internal Guid JobId { get; set; }
    public required Guid ProblemId { get; set; }
    public required string CodeB64 { get; set; }
}
