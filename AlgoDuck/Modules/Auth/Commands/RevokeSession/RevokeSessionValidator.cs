using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.RevokeSession;

public sealed class RevokeSessionValidator : AbstractValidator<RevokeSessionDto>
{
    public RevokeSessionValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
    }
}