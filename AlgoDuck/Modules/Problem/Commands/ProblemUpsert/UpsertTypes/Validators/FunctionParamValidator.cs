using FluentValidation;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes.Validators;

public class FunctionParamValidator : AbstractValidator<FunctionParam>
{
    public FunctionParamValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Parameter type is required.")
            .MaximumLength(512).WithMessage("Parameter type cannot exceed 512 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Parameter name is required.")
            .MaximumLength(256).WithMessage("Parameter name cannot exceed 256 characters.")
            .Matches(@"^[a-zA-Z_][a-zA-Z0-9_]*$").WithMessage("Parameter name must be a valid identifier.");
    }
}