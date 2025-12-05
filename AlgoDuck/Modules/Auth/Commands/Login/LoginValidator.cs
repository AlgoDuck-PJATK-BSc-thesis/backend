using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.Login;

public sealed class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.UserNameOrEmail)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(256);
    }
}