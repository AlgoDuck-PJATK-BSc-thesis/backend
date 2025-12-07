using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.Register;

public sealed class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(256)
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.Password)
            .WithMessage("Password and confirmation password do not match.");
    }
}