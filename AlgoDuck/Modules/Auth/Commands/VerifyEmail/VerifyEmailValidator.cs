using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailValidator : AbstractValidator<VerifyEmailDto>
{
    public VerifyEmailValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Token)
            .NotEmpty();
    }
}