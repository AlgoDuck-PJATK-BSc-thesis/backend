using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.RevokeOtherSessions;

public sealed class RevokeOtherSessionsValidator : AbstractValidator<RevokeOtherSessionsDto>
{
    public RevokeOtherSessionsValidator()
    {
        RuleFor(x => x.CurrentSessionId).NotEmpty();
    }
}