using FluentValidation;

namespace AlgoDuck.Modules.Auth.Queries.GetApiKeys;

public sealed class GetApiKeysValidator : AbstractValidator<Guid>
{
    public GetApiKeysValidator()
    {
        RuleFor(x => x).NotEmpty();
    }
}