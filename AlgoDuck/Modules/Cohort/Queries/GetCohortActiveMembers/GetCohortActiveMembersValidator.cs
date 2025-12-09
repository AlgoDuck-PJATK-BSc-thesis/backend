using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries.GetCohortActiveMembers;

public sealed class GetCohortActiveMembersValidator : AbstractValidator<GetCohortActiveMembersRequestDto>
{
    public GetCohortActiveMembersValidator()
    {
        RuleFor(x => x.CohortId)
            .NotEmpty();
    }
}