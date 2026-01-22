using FluentValidation;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes.Validators;

public class ProblemTagValidator : AbstractValidator<ProblemTagDto>
{
    public ProblemTagValidator()
    {
        RuleFor(x => x.TagName)
            .NotEmpty().WithMessage("Tag name is required.")
            .MaximumLength(256).WithMessage("Tag name cannot exceed 256 characters.");
    }
}