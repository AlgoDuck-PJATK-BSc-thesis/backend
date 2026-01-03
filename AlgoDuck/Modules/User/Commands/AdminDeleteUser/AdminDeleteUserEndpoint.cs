using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Commands.AdminDeleteUser;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "admin")]
public sealed class AdminDeleteUserEndpoint : ControllerBase
{
    private readonly IAdminDeleteUserHandler _handler;

    public AdminDeleteUserEndpoint(IAdminDeleteUserHandler handler)
    {
        _handler = handler;
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        await _handler.HandleAsync(userId, cancellationToken);
        return Ok();
    }
}