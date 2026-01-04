using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.Session.RevokeOtherSessions;

public sealed class RevokeOtherSessionsValidator : AbstractValidator<RevokeOtherSessionsDto>
{
    public RevokeOtherSessionsValidator()
    {
        RuleFor(x => x.CurrentSessionId).NotEmpty();
    }
}