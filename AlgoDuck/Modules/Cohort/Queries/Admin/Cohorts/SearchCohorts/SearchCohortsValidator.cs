using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.SearchCohorts;

public sealed class SearchCohortsValidator : AbstractValidator<SearchCohortsDto>
{
    public SearchCohortsValidator()
    {
        RuleFor(x => x.Query).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(200);
    }
}