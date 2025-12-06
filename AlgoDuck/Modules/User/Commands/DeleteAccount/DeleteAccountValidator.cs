using FluentValidation;

namespace AlgoDuck.Modules.User.Commands.DeleteAccount;

public sealed class DeleteAccountValidator : AbstractValidator<DeleteAccountDto>
{
    public DeleteAccountValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .MinimumLength(6);
    }
}