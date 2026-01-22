using FluentValidation;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes.Validators;

public class UpsertProblemValidator : AbstractValidator<UpsertProblemDto>
{
    private const int MaxCodeLengthBytes = 128 * 1024;

    public UpsertProblemValidator(
        IValidator<TestCaseDto> testCaseValidator,
        IValidator<ProblemTagDto> tagValidator)
    {
        RuleFor(x => x.TemplateB64)
            .NotEmpty().WithMessage("Problem template is required.")
            .MaximumLength(MaxCodeLengthBytes).WithMessage($"Problem template cannot exceed {MaxCodeLengthBytes} bytes.");

        RuleFor(x => x.ProblemTitle)
            .NotEmpty().WithMessage("Problem title is required.")
            .MaximumLength(256).WithMessage("Problem title cannot exceed 256 characters.");

        RuleFor(x => x.ProblemDescription)
            .NotEmpty().WithMessage("Problem description is required.")
            .MaximumLength(32 * 1024).WithMessage("Problem description cannot exceed 32KB.");

        RuleFor(x => x.DifficultyId)
            .NotEmpty().WithMessage("Difficulty is required.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Tags)
            .Must(t => t.Count <= 20).WithMessage("Cannot have more than 20 tags.");

        RuleForEach(x => x.Tags)
            .SetValidator(tagValidator);

        RuleFor(x => x.TestCases)
            .NotEmpty().WithMessage("At least one test case is required.")
            .Must(tc => tc.Count <= 100).WithMessage("Cannot have more than 100 test cases.");

        RuleForEach(x => x.TestCases)
            .SetValidator(testCaseValidator);
    }
}