using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.ChangeEmailConfirm;

public sealed class ChangeEmailConfirmValidator : AbstractValidator<ChangeEmailConfirmDto>
{
    public ChangeEmailConfirmValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
    }
}