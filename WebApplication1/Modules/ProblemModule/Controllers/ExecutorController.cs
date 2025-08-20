using Microsoft.AspNetCore.Mvc;
using WebApplication1.Modules.ProblemModule.DTOs;
using WebApplication1.Modules.ProblemModule.DTOs.ExecutorDtos;
using WebApplication1.Modules.ProblemModule.Interfaces;

namespace WebApplication1.Modules.ProblemModule.Controllers;

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