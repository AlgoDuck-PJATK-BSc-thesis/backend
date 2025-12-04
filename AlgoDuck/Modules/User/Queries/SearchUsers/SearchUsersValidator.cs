using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.SearchUsers;

public sealed class SearchUsersValidator : AbstractValidator<SearchUsersDto>
{
    public SearchUsersValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Query)
            .MaximumLength(256);
    }
}