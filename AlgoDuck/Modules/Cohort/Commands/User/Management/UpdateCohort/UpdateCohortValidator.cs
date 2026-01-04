using AlgoDuck.Modules.Cohort.Shared.Validators;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.User.Management.UpdateCohort;

public sealed class UpdateCohortValidator : AbstractValidator<UpdateCohortDto>
{
    public UpdateCohortValidator()
    {
        RuleFor(x => x.Name)
            .SetValidator(new CohortNameValidator());
    }
}