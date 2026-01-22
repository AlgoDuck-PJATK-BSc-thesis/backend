using FluentValidation;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes.Validators;

public class MethodRecommendationValidator : AbstractValidator<MethodRecommendation>
{
    public MethodRecommendationValidator(IValidator<FunctionParam> functionParamValidator)
    {
        RuleFor(x => x.MethodName)
            .NotEmpty().WithMessage("Method name is required.")
            .MaximumLength(256).WithMessage("Method name cannot exceed 256 characters.")
            .Matches(@"^[a-zA-Z_][a-zA-Z0-9_]*$").WithMessage("Method name must be a valid identifier.");

        RuleFor(x => x.QualifiedName)
            .NotEmpty().WithMessage("Qualified name is required.")
            .MaximumLength(1024).WithMessage("Qualified name cannot exceed 1024 characters.");

        RuleFor(x => x.FunctionParams)
            .Must(p => p.Count <= 20).WithMessage("Cannot have more than 20 function parameters.");

        RuleForEach(x => x.FunctionParams)
            .SetValidator(functionParamValidator);

        RuleFor(x => x.Generics)
            .Must(g => g.Count <= 10).WithMessage("Cannot have more than 10 generic parameters.");

        RuleFor(x => x.Modifiers)
            .Must(m => m.Count <= 10).WithMessage("Cannot have more than 10 modifiers.");

        RuleFor(x => x.AccessModifier)
            .NotEmpty().WithMessage("Access modifier is required.")
            .MaximumLength(32).WithMessage("Access modifier cannot exceed 32 characters.");

        RuleFor(x => x.ReturnType)
            .NotEmpty().WithMessage("Return type is required.")
            .MaximumLength(512).WithMessage("Return type cannot exceed 512 characters.");
    }
}