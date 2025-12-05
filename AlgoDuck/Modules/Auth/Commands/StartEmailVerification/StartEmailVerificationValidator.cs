using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.StartEmailVerification;

public sealed class StartEmailVerificationValidator : AbstractValidator<StartEmailVerificationDto>
{
    public StartEmailVerificationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(256)
            .EmailAddress();
    }
}