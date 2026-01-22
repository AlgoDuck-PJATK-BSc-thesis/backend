using AlgoDuck.Modules.Problem.Shared.Types;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Problem.Commands.AutoSaveUserCode;

public class AutoSaveValidator : AbstractValidator<AutoSaveDto>
{
    public AutoSaveValidator(IOptions<ValidationConfig> config)
    {
        RuleFor(a => a.UserCodeB64)
            .NotEmpty().WithMessage("Code is required for autosave")
            .MaximumLength(config.Value.MaxCodeBytes).WithMessage($"Code length must be less than {config.Value.MaxCodeBytes} bytes");
    }
}