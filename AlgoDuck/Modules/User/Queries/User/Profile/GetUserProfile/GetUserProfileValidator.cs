using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.User.Profile.GetUserProfile;

public sealed class GetUserProfileValidator : AbstractValidator<Guid>
{
    public GetUserProfileValidator()
    {
        RuleFor(x => x).NotEmpty();
    }
}