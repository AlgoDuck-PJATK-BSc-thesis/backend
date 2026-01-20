using FluentValidation;

namespace AlgoDuck.Modules.Auth.Queries.Sessions.GetUserSessions;

public sealed class GetUserSessionsValidator : AbstractValidator<Guid>
{
    public GetUserSessionsValidator()
    {
        RuleFor(x => x).NotEmpty();
    }
}