namespace AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;

using AlgoDuck.Modules.Problem.Shared.Types;
using FluentValidation;
using Microsoft.Extensions.Options;

public class CodeExecutionValidator : AbstractValidator<DryRunExecuteRequest>
{
    public CodeExecutionValidator(IOptions<ValidationConfig> config)
    {
        RuleFor(a => a.CodeB64)
            .NotEmpty().WithMessage("Code is required submission")
            .MaximumLength(config.Value.MaxCodeBytes).WithMessage($"Code length must be less than {config.Value.MaxCodeBytes} bytes");
    }
}