using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.GetUserActivity;

public sealed class GetUserActivityValidator : AbstractValidator<GetUserActivityRequestDto>
{
    public GetUserActivityValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}