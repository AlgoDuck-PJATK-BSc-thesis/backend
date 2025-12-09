using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Shared.Validators;

public sealed class CohortNameValidator : AbstractValidator<string>
{
    public CohortNameValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(64);
    }
}