using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.AdminGetUsers;

public sealed class AdminGetUsersValidator : AbstractValidator<AdminGetUsersDto>
{
    public AdminGetUsersValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(200);
    }
}