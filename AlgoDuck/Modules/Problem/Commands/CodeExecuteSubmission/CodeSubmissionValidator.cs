using AlgoDuck.Modules.Problem.Shared.Types;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;

public class CodeSubmissionValidator : AbstractValidator<SubmitExecuteRequest>
{
    public CodeSubmissionValidator(IOptions<ValidationConfig> config)
    {
        RuleFor(a => a.CodeB64)
            .NotEmpty().WithMessage("Code is required submission")
            .MaximumLength(config.Value.MaxCodeBytes).WithMessage($"Code length must be less than {config.Value.MaxCodeBytes} bytes");
    }
}