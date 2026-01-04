using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.CreateCohort;

public sealed class CreateCohortValidator : AbstractValidator<CreateCohortDto>
{
    public CreateCohortValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
    }
}