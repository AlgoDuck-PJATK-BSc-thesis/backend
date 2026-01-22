using AlgoDuck.Modules.Problem.Shared.Types;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Problem.Commands.QueryAssistant;

public class QueryValidator : AbstractValidator<AssistantRequestDto>
{
    public QueryValidator(IOptions<ValidationConfig> config)
    {
        RuleFor(x => x.CodeB64 ).NotEmpty().WithMessage("Request must contain code").MaximumLength(config.Value.MaxCodeBytes).WithMessage($"Payload may not be greater than {config.Value.MaxCodeBytes} bytes");
        RuleFor(x => x.Query).NotEmpty().WithMessage("Request must contain query").MaximumLength(64 * 1024).WithMessage($"Message may not be larger than {64 * 1024} characters");
    }
}