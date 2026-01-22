using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Shared.Validators;

public sealed class CohortNameValidator : AbstractValidator<string>
{
    public CohortNameValidator()
    {
        RuleFor(x => x).Custom((value, context) =>
        {
            var trimmed = (value ?? string.Empty).Trim();
            var len = trimmed.Length;

            if (len == 0)
            {
                context.AddFailure("Cohort's name is required.");
                return;
            }

            if (len < 3)
            {
                context.AddFailure($"The length of Cohort's name must be at least 3 characters. You entered {len} characters.");
                return;
            }

            if (len > 256)
            {
                context.AddFailure($"The length of Cohort's name must be 256 characters or fewer. You entered {len} characters.");
            }
        });
    }
}