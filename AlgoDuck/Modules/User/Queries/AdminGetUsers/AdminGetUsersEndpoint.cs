using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.AdminGetUsers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "admin")]
public sealed class AdminGetUsersEndpoint : ControllerBase
{
    private readonly IAdminGetUsersHandler _handler;
    private readonly IValidator<AdminGetUsersDto> _validator;

    public AdminGetUsersEndpoint(IAdminGetUsersHandler handler, IValidator<AdminGetUsersDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AdminGetUsersDto request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _handler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }
}