using FluentValidation;

namespace AlgoDuck.Modules.User.Commands.UpdateUsername;

public sealed class UpdateUsernameValidator : AbstractValidator<UpdateUsernameDto>
{
    public UpdateUsernameValidator()
    {
        RuleFor(x => x.NewUserName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(32)
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Username can only contain letters, digits, and underscores.");
    }
}