using System.Security.Claims;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetUserById;

[ApiController]
[Route("api/user/me")]
[Authorize]
public sealed class GetMyUserEndpoint : ControllerBase
{
    private readonly IGetUserByIdHandler _handler;

    public GetMyUserEndpoint(IGetUserByIdHandler handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
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

        var user = await _handler.HandleAsync(new GetUserByIdRequestDto { UserId = userId }, cancellationToken);

        return Ok(new StandardApiResponse<UserDto>
        {
            Body = user
        });
    }
}