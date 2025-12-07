using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.ResetPassword;

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Token)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.Password)
            .WithMessage("Password and confirmation password do not match.");
    }
}