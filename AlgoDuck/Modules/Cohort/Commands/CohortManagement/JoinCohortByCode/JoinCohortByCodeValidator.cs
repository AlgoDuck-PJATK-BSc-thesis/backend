using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.JoinCohortByCode;

public sealed class JoinCohortByCodeValidator : AbstractValidator<JoinCohortByCodeDto>
{
    public JoinCohortByCodeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(16)
            .Matches("^[A-Z0-9]+$");
    }
}