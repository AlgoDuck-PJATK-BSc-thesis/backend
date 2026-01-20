using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.CreateEmptyAssistantChat;

[ApiController]
[Authorize]
[Route("api/problem/assistant/chat")]
public class EmptyAssistantChatController : ControllerBase
{
    private readonly ICreateEmptyAssistantChatService _service;

    public EmptyAssistantChatController(ICreateEmptyAssistantChatService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewChatAsync([FromQuery] Guid problemId, CancellationToken cancellationToken)
    {
        return await User.GetUserId().BindAsync(async userId => await _service.CreateEmptyAssistantChatAsync(
            new EmptyAssistantChatRequest
            {
                ProblemId = problemId,
                UserId = userId
            }, cancellationToken)).ToActionResultAsync();
    }
}


public class EmptyAssistantChatRequest
{
    public required Guid UserId { get; set; }
    public required Guid ProblemId { get; set; }
}

public class AssistantChatCreationDto
{
    public required Guid ChatId { get; set; }
}