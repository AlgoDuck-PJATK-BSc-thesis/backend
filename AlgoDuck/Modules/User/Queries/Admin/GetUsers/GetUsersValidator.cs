using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.Admin.GetUsers;

public sealed class GetUsersValidator : AbstractValidator<GetUsersDto>
{
    public GetUsersValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(200);
    }
}