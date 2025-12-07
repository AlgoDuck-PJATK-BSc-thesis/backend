using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.RefreshToken;

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}