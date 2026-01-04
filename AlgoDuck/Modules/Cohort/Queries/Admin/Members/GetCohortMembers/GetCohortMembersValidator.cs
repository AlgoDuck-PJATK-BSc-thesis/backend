using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries.Admin.Members.GetCohortMembers;

public sealed class GetCohortMembersValidator : AbstractValidator<GetCohortMembersRequestDto>
{
    public GetCohortMembersValidator()
    {
        RuleFor(x => x.CohortId).NotEmpty();
    }
}