using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetTwoFactorEnabled;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class GetTwoFactorEnabledEndpoint : ControllerBase
{
    private readonly IGetTwoFactorEnabledHandler _handler;

    public GetTwoFactorEnabledEndpoint(IGetTwoFactorEnabledHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("{userId:guid}/two-factor-status")]
    public async Task<ActionResult<TwoFactorStatusDto>> Get(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(userId, cancellationToken);
        return Ok(result);
    }
}