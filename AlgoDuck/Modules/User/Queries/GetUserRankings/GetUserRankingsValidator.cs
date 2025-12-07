using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.GetUserRankings;

public sealed class GetUserRankingsValidator : AbstractValidator<GetUserRankingsQuery>
{
    private const int MinPage = 1;
    private const int MinPageSize = 1;
    private const int MaxPageSize = 100;

    public GetUserRankingsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(MinPage);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(MinPageSize, MaxPageSize);
    }
}