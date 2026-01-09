using FluentValidation;

namespace AlgoDuck.Modules.Auth.Queries.Identity.SearchUsersByEmail;

public sealed class SearchUsersByEmailValidator : AbstractValidator<SearchUsersByEmailDto>
{
    public SearchUsersByEmailValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(256);

        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}