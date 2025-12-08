using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetVerifiedEmail;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class GetVerifiedEmailController : ControllerBase
{
    private readonly IGetVerifiedEmailHandler _handler;

    public GetVerifiedEmailController(IGetVerifiedEmailHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("{userId:guid}/email-status")]
    public async Task<ActionResult<GetVerifiedEmailResultDto>> Get(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(userId, cancellationToken);
        return Ok(result);
    }
}