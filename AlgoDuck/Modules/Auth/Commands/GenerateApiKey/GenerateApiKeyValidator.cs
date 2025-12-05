using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.GenerateApiKey;

public sealed class GenerateApiKeyValidator : AbstractValidator<GenerateApiKeyDto>
{
    public GenerateApiKeyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.LifetimeDays)
            .GreaterThan(0)
            .When(x => x.LifetimeDays.HasValue);
    }
}