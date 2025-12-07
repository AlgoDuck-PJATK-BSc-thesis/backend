using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.RevokeApiKey;

public sealed class RevokeApiKeyValidator : AbstractValidator<RevokeApiKeyDto>
{
    public RevokeApiKeyValidator()
    {
    }
}