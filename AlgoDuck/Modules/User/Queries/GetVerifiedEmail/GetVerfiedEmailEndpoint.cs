using System.Security.Claims;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetVerifiedEmail;

[ApiController]
[Authorize]
public sealed class GetVerifiedEmailController : ControllerBase
{
    private readonly IGetVerifiedEmailHandler _handler;

    public GetVerifiedEmailController(IGetVerifiedEmailHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("api/users/{userId:guid}/email-status")]
    public async Task<IActionResult> Get(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(userId, cancellationToken);

        return Ok(new StandardApiResponse<GetVerifiedEmailResultDto>
        {
            Status = Status.Success,
            Body = result,
            Message = ""
        });
    }

    [HttpGet("api/user/email")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Unauthorized"
            });
        }

        var result = await _handler.HandleAsync(userId, cancellationToken);

        return Ok(new StandardApiResponse<GetVerifiedEmailResultDto>
        {
            Status = Status.Success,
            Body = result,
            Message = ""
        });
    }
}