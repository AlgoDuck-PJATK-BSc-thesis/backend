using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Queries.ValidateToken;

[ApiController]
[Route("api/auth/validate-token")]
public sealed class ValidateTokenEndpoint : ControllerBase
{
    private readonly IValidateTokenHandler _handler;
    private readonly IValidator<ValidateTokenDto> _validator;

    public ValidateTokenEndpoint(
        IValidateTokenHandler handler,
        IValidator<ValidateTokenDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ValidateTokenResult>> ValidateToken(ValidateTokenDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var result = await _handler.HandleAsync(dto, cancellationToken);
        return Ok(result);
    }
}