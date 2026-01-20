using FluentValidation;

namespace AlgoDuck.Modules.User.Commands.User.Account.DeleteAccount;

public sealed class DeleteAccountValidator : AbstractValidator<DeleteAccountDto>
{
    private const string Phrase = "I am sure I want to delete my account";

    public DeleteAccountValidator()
    {
        RuleFor(x => x.ConfirmationText)
            .NotEmpty()
            .Must(v => (v ?? string.Empty).Trim() == Phrase);
    }
}