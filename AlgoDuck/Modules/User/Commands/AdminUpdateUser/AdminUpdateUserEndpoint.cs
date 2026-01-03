using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Commands.AdminUpdateUser;

[ApiController]
[Route("api/admin/users/{userId:guid}")]
[Authorize(Roles = "admin")]
public sealed class AdminUpdateUserEndpoint : ControllerBase
{
    private readonly IAdminUpdateUserHandler _handler;

    public AdminUpdateUserEndpoint(IAdminUpdateUserHandler handler)
    {
        _handler = handler;
    }

    [HttpPut]
    public async Task<ActionResult<AdminUpdateUserResultDto>> UpdateAsync(Guid userId, [FromBody] AdminUpdateUserDto dto, CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(userId, dto, cancellationToken);
        return Ok(result);
    }
}