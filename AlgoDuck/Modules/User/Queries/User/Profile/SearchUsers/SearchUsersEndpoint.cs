using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.User.Profile.SearchUsers;

[ApiController]
[Route("api/user/search")]
[Authorize]
public sealed class SearchUsersEndpoint : ControllerBase
{
    private readonly ISearchUsersHandler _handler;
    private readonly IValidator<SearchUsersDto> _validator;

    public SearchUsersEndpoint(
        ISearchUsersHandler handler,
        IValidator<SearchUsersDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] SearchUsersDto request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var result = await _handler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }
}