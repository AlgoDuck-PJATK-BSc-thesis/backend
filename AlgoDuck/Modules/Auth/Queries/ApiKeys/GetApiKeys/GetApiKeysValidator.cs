using FluentValidation;

namespace AlgoDuck.Modules.Auth.Queries.ApiKeys.GetApiKeys;

public sealed class GetApiKeysValidator : AbstractValidator<Guid>
{
    public GetApiKeysValidator()
    {
        RuleFor(x => x).NotEmpty();
    }
}