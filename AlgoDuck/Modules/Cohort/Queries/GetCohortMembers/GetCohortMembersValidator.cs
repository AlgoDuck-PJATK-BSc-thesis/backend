using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries.GetCohortMembers;

public sealed class GetCohortMembersValidator : AbstractValidator<GetCohortMembersRequestDto>
{
    public GetCohortMembersValidator()
    {
        RuleFor(x => x.CohortId)
            .NotEmpty();

        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}