using AlgoDuck.Modules.Cohort.DTOs;
using AlgoDuck.Modules.Cohort.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AlgoDuck.Shared.Http;

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
    public async Task<IActionResult> GetMessages(Guid cohortId)
    {
        return Ok(new StandardApiResponse<List<CohortChatDto>>
        {
            Body = await _chatService.GetMessagesAsync(cohortId)
        });
    }
}