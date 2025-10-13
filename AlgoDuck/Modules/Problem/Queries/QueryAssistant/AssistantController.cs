using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.QueryAssistant;

[ApiController]
[Route("api/[controller]")]
public class AssistantController(IAssistantService assistant) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> GetAssistance([FromBody] AssistantRequestDto assistantRequest)
    {
        return Ok(await assistant.GetAssistanceAsync(assistantRequest));
    }
}