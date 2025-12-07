using AlgoDuck.Modules.Auth.Shared.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Queries.SearchUsersByEmail;

[ApiController]
[Route("api/auth/search-users")]
public sealed class SearchUsersByEmailEndpoint : ControllerBase
{
    private readonly ISearchUsersByEmailHandler _handler;
    private readonly IValidator<SearchUsersByEmailDto> _validator;

    public SearchUsersByEmailEndpoint(
        ISearchUsersByEmailHandler handler,
        IValidator<SearchUsersByEmailDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<AuthUserDto>>> Search(SearchUsersByEmailDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var result = await _handler.HandleAsync(dto, cancellationToken);
        return Ok(result);
    }
}