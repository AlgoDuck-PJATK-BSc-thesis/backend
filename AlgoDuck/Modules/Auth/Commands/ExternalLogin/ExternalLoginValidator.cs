using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.ExternalLogin;

public sealed class ExternalLoginValidator : AbstractValidator<ExternalLoginDto>
{
    public ExternalLoginValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.ExternalUserId)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(256)
            .EmailAddress();

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(256);
    }
}