using AlgoDuck.Modules.Auth.Shared.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Queries.GetUserByToken;

[ApiController]
[Route("api/auth/user-by-token")]
public sealed class GetUserByTokenEndpoint : ControllerBase
{
    private readonly IGetUserByTokenHandler _handler;
    private readonly IValidator<UserByTokenDto> _validator;

    public GetUserByTokenEndpoint(
        IGetUserByTokenHandler handler,
        IValidator<UserByTokenDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<AuthUserDto>> GetUserByToken(UserByTokenDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var user = await _handler.HandleAsync(dto, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}