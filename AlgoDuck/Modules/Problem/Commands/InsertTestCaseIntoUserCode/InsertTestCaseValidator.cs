using AlgoDuck.Modules.Problem.Shared.Types;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Problem.Commands.InsertTestCaseIntoUserCode;

public class InsertTestCaseValidator : AbstractValidator<InsertRequestDto>
{
    public InsertTestCaseValidator(IOptions<ValidationConfig> config)
    {
        RuleFor(x => x.UserCodeB64).NotEmpty().WithMessage("Request must contain code").MaximumLength(config.Value.MaxCodeBytes).WithMessage($"Payload may not be greater than {config.Value.MaxCodeBytes} bytes");
    }
}