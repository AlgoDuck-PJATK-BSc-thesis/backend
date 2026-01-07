using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Commands.Admin.CreateUser;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "admin")]
public sealed class CreateUserEndpoint : ControllerBase
{
    private readonly ICreateUserHandler _handler;

    public CreateUserEndpoint(ICreateUserHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<ActionResult<CreateUserResultDto>> CreateAsync([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var result = await _handler.HandleAsync(dto, cancellationToken);
        return Ok(result);
    }
}
