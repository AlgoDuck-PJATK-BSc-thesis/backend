using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.UpdateCohort;

public sealed class UpdateCohortValidator : AbstractValidator<UpdateCohortDto>
{
    public UpdateCohortValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
    }
}