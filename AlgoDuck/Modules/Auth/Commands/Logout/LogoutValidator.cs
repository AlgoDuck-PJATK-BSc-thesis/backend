using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.Logout;

public sealed class LogoutValidator : AbstractValidator<LogoutDto>
{
    public LogoutValidator()
    {
        RuleFor(x => x.SessionId)
            .Must(x => !x.HasValue || x.Value != Guid.Empty);
    }
}