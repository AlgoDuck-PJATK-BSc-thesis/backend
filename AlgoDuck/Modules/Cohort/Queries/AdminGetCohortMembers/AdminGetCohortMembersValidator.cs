using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries.AdminGetCohortMembers;

public sealed class AdminGetCohortMembersValidator : AbstractValidator<AdminGetCohortMembersRequestDto>
{
    public AdminGetCohortMembersValidator()
    {
        RuleFor(x => x.CohortId).NotEmpty();
    }
}