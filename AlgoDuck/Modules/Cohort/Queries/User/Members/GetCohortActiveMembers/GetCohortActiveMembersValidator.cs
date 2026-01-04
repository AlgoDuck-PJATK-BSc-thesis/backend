using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries.User.Members.GetCohortActiveMembers;

public sealed class GetCohortActiveMembersValidator : AbstractValidator<GetCohortActiveMembersRequestDto>
{
    public GetCohortActiveMembersValidator()
    {
        RuleFor(x => x.CohortId)
            .NotEmpty();
    }
}