using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Commands.Chat.SendMessage;

[ApiController]
[Route("api/cohorts/{cohortId:guid}/chat/messages")]
[Authorize]
public sealed class SendMessageEndpoint : ControllerBase
{
    private readonly ISendMessageHandler _handler;

    public SendMessageEndpoint(ISendMessageHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<ActionResult<SendMessageResultDto>> SendAsync(
        Guid cohortId,
        [FromBody] SendMessageDto dto)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var effectiveDto = new SendMessageDto
        {
            CohortId = cohortId,
            Content = dto.Content,
            MediaType = dto.MediaType,
            MediaKey = dto.MediaKey,
            MediaContentType = dto.MediaContentType
        };

        var result = await _handler.HandleAsync(userId, effectiveDto, CancellationToken.None);
        return Ok(result);
    }
}