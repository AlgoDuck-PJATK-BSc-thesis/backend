using AlgoDuck.Modules.Problem.DTOs.ExecutorDtos;
using AlgoDuck.Modules.Problem.Interfaces;
using Microsoft.AspNetCore.Mvc;
using AlgoDuck.Modules.Problem.DTOs;

namespace AlgoDuck.Modules.Problem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExecutorController(IExecutorService executorService) : ControllerBase
{
    [HttpPost("full")]
    public async Task<IActionResult> ExecuteCode([FromBody] ExecuteRequestDto executeRequest)
    {
        return Ok(await executorService.FullExecuteCode(executeRequest));
    }
    [HttpPost("dry")]
    public async Task<IActionResult> DryExecuteCode([FromBody] DryExecuteRequestDto executeRequest)
    {
        return Ok(await executorService.DryExecuteCode(executeRequest));
    }

}