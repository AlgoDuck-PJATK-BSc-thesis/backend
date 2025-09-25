using AlgoDuck.Modules.Problem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssistantController : ControllerBase
{
    private readonly IAssistantService _assistant;

    public AssistantController(IAssistantService assistant)
    {
        _assistant = assistant;
    }

    [HttpPost]
    public async Task<IActionResult> GetAssistance([FromBody] AssistantRequestDto assistantRequest)
    {
        return Ok(await _assistant.GetAssistanceAsync(assistantRequest));
    }
}