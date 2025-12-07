using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.ChangeEmailRequest;

public sealed class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequestDto>
{
    public ChangeEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .EmailAddress();
    }
}