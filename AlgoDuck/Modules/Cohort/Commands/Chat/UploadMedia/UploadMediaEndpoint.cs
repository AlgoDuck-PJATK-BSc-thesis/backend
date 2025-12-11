using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Commands.Chat.UploadMedia;

[ApiController]
[Route("api/cohorts/{cohortId:guid}/chat/media")]
[Authorize]
public sealed class UploadMediaEndpoint : ControllerBase
{
    private readonly IUploadMediaHandler _handler;

    public UploadMediaEndpoint(IUploadMediaHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    [RequestSizeLimit(64 * 1024 * 1024)]
    public async Task<ActionResult<UploadMediaResultDto>> UploadAsync(
        Guid cohortId,
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        if (file is null)
        {
            return BadRequest("File is required.");
        }

        var dto = new UploadMediaDto
        {
            CohortId = cohortId,
            File = file
        };

        var result = await _handler.HandleAsync(userId, dto, cancellationToken);
        return Ok(result);
    }
}