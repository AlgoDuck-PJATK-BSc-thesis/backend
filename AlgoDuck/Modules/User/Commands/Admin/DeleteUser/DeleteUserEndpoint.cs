using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Commands.DeleteUser;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "admin")]
public sealed class DeleteUserEndpoint : ControllerBase
{
    private readonly IDeleteUserHandler _handler;

    public DeleteUserEndpoint(IDeleteUserHandler handler)
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