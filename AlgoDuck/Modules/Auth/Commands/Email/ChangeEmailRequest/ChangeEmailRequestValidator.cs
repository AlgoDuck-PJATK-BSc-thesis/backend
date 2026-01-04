using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.Email.ChangeEmailRequest;

public sealed class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequestDto>
{
    public ChangeEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .EmailAddress();
    }
}