using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries.AdminSearchCohorts;

public sealed class AdminSearchCohortsValidator : AbstractValidator<AdminSearchCohortsDto>
{
    public AdminSearchCohortsValidator()
    {
        RuleFor(x => x.Query).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(200);
    }
}