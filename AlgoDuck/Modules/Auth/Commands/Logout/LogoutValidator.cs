using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.Logout;

public sealed class LogoutValidator : AbstractValidator<LogoutDto>
{
    public LogoutValidator()
    {
    }
}