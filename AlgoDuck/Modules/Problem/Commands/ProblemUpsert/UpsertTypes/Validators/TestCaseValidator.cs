using FluentValidation;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes.Validators;

public class TestCaseValidator : AbstractValidator<TestCaseDto>
{
    public TestCaseValidator(
        IValidator<MethodRecommendation> methodValidator,
        IValidator<FunctionParam> functionParamValidator)
    {
        RuleFor(x => x.Display)
            .NotEmpty().WithMessage("Display is required.")
            .MaximumLength(16 * 1024).WithMessage("Display cannot exceed 16KB.");

        RuleFor(x => x.DisplayRes)
            .NotEmpty().WithMessage("Display result is required.")
            .MaximumLength(16 * 1024).WithMessage("Display result cannot exceed 16KB.");

        RuleFor(x => x.ArrangeB64)
            .NotEmpty().WithMessage("Arrange is required.")
            .MinimumLength(8).WithMessage("Arrange must be at least 8 characters.")
            .MaximumLength(16 * 1024).WithMessage("Arrange cannot exceed 16KB.");

        RuleFor(x => x.CallMethod)
            .NotNull().WithMessage("Call method is required.")
            .SetValidator(methodValidator);

        RuleFor(x => x.CallArgs)
            .Must(a => a.Count <= 50).WithMessage("Cannot have more than 50 call arguments.");

        RuleForEach(x => x.CallArgs)
            .SetValidator(functionParamValidator);

        RuleFor(x => x.Expected)
            .NotNull().WithMessage("Expected value is required.")
            .SetValidator(functionParamValidator);
    }
}