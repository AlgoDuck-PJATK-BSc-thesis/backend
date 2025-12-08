using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetSelectedAvatar;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class GetSelectedAvatarEndpoint : ControllerBase
{
    private readonly IGetSelectedAvatarHandler _handler;

    public GetSelectedAvatarEndpoint(IGetSelectedAvatarHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("{userId:guid}/selected-avatar")]
    public async Task<ActionResult<SelectedAvatarDto>> Get(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(userId, cancellationToken);
        return Ok(result);
    }
}