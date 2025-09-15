using AlgoDuck.Modules.Cohort.DTOs;
using AlgoDuck.Modules.Cohort.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Controllers;

[ApiController]
[Route("api/cohorts/{cohortId:guid}/chat/messages")]
[Authorize]
public class CohortChatController : ControllerBase
{
    private readonly ICohortChatService _chatService;

    public CohortChatController(ICohortChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CohortChatDto>>> GetMessages(Guid cohortId)
    {
        var messages = await _chatService.GetMessagesAsync(cohortId);
        return Ok(messages);
    }
}