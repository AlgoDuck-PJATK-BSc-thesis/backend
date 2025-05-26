using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Modules.CohortModule.Chat.DTOs;
using WebApplication1.Modules.CohortModule.Chat.Interfaces;

namespace WebApplication1.Modules.CohortModule.Chat.Controllers;

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