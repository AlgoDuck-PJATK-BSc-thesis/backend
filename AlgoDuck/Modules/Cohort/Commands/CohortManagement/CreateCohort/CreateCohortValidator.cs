using AlgoDuck.Modules.Cohort.Shared.Validators;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.CreateCohort;

public sealed class CreateCohortValidator : AbstractValidator<CreateCohortDto>
{
    public CreateCohortValidator()
    {
        RuleFor(x => x.Name)
            .SetValidator(new CohortNameValidator());
    }
}