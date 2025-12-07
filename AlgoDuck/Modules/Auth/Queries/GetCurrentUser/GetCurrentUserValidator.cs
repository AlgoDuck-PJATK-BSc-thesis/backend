using FluentValidation;

namespace AlgoDuck.Modules.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserValidator : AbstractValidator<Guid>
{
    public GetCurrentUserValidator()
    {
        RuleFor(x => x).NotEmpty();
    }
}