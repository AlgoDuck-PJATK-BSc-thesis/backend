using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries.User.Cohorts.GetCohortById;

public sealed class GetCohortByIdValidator : AbstractValidator<GetCohortByIdRequestDto>
{
    public GetCohortByIdValidator()
    {
        RuleFor(x => x.CohortId)
            .NotEmpty();
    }
}