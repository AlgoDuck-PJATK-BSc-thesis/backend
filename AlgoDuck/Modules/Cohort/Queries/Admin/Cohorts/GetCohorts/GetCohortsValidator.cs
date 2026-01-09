using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.GetCohorts;


public sealed class AdminGetCohortsValidator : AbstractValidator<AdminGetCohortsDto>
{
    public AdminGetCohortsValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(200);
    }
}