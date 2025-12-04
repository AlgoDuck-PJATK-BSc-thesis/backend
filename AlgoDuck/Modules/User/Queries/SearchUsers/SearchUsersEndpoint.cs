using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.SearchUsers;

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
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var result = await _handler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }
}