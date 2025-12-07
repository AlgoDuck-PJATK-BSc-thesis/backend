using FluentValidation;

namespace AlgoDuck.Modules.Auth.Queries.ValidateToken;

public sealed class ValidateTokenValidator : AbstractValidator<ValidateTokenDto>
{
    public ValidateTokenValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty();
    }
}