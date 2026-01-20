using FluentValidation;

namespace AlgoDuck.Modules.Auth.Queries.Token.ValidateToken;

public sealed class ValidateTokenValidator : AbstractValidator<ValidateTokenDto>
{
    public ValidateTokenValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty();
    }
}