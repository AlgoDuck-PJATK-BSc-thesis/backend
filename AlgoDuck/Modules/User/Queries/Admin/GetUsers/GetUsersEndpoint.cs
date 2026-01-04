using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.Admin.GetUsers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "admin")]
public sealed class GetUsersEndpoint : ControllerBase
{
    private readonly IGetUsersHandler _handler;
    private readonly IValidator<GetUsersDto> _validator;

    public GetUsersEndpoint(IGetUsersHandler handler, IValidator<GetUsersDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetUsersDto request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _handler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }
}