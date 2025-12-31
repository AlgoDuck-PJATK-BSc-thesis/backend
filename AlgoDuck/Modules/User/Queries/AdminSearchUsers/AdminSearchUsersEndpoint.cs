using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.AdminSearchUsers;

[ApiController]
[Route("api/admin/users/search")]
[Authorize(Roles = "admin")]
public sealed class AdminSearchUsersEndpoint : ControllerBase
{
    private readonly IAdminSearchUsersHandler _handler;
    private readonly IValidator<AdminSearchUsersDto> _validator;

    public AdminSearchUsersEndpoint(IAdminSearchUsersHandler handler, IValidator<AdminSearchUsersDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AdminSearchUsersDto request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _handler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }
}