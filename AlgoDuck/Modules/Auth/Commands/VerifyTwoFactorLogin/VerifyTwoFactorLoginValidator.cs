using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.VerifyTwoFactorLogin;

public sealed class VerifyTwoFactorLoginValidator : AbstractValidator<VerifyTwoFactorLoginDto>
{
    public VerifyTwoFactorLoginValidator()
    {
        RuleFor(x => x.ChallengeId)
            .NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6);
    }
}