using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Commands.Admin.UpdateUser;

[ApiController]
[Route("api/admin/users/{userId:guid}")]
[Authorize(Roles = "admin")]
public sealed class UpdateUserEndpoint : ControllerBase
{
    private readonly IUpdateUserHandler _handler;

    public UpdateUserEndpoint(IUpdateUserHandler handler)
    {
        _handler = handler;
    }

    [HttpPut]
    public async Task<ActionResult<UpdateUserResultDto>> UpdateAsync(Guid userId, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(userId, dto, cancellationToken);
        return Ok(result);
    }
}