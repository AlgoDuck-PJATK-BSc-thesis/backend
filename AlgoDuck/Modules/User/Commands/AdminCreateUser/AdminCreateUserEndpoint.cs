using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Commands.AdminCreateUser;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "admin")]
public sealed class AdminCreateUserEndpoint : ControllerBase
{
    private readonly IAdminCreateUserHandler _handler;

    public AdminCreateUserEndpoint(IAdminCreateUserHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<ActionResult<AdminCreateUserResultDto>> CreateAsync([FromBody] AdminCreateUserDto dto, CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(dto, cancellationToken);
        return Ok(result);
    }
}