using FluentValidation;

namespace AlgoDuck.Modules.User.Commands.CreateUser;

public sealed class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(200);

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r =>
            {
                var v = r.Trim().ToLowerInvariant();
                return v == "user" || v == "admin";
            });

        When(x => x.Username is not null, () =>
        {
            RuleFor(x => x.Username!)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(64)
                .Matches("^[A-Za-z0-9_]+$");
        });
    }
}