using System.Security.Claims;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Http;
using AlgoDuckShared.Executor.SharedTypes;
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
        
        return Ok(new StandardApiResponse<ExecuteResponse>
        {
            Body = await executorService.SubmitUserCodeAsync(executeRequest, Guid.Parse(userId))
        });
    }
}