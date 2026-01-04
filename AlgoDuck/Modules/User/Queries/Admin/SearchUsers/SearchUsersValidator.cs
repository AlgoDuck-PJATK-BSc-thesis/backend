using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.Admin.SearchUsers;

public sealed class SearchUsersValidator : AbstractValidator<SearchUsersDto>
{
    public SearchUsersValidator()
    {
        RuleFor(x => x.Query).NotEmpty();

        RuleFor(x => x.UsernamePage).GreaterThanOrEqualTo(1);
        RuleFor(x => x.UsernamePageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(200);

        RuleFor(x => x.EmailPage).GreaterThanOrEqualTo(1);
        RuleFor(x => x.EmailPageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(200);
    }
}