using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.GetUserProfile;

public sealed class GetUserProfileValidator : AbstractValidator<Guid>
{
    public GetUserProfileValidator()
    {
        RuleFor(x => x).NotEmpty();
    }
}