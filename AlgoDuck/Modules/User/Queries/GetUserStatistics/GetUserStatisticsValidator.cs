using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.GetUserStatistics;

public sealed class GetUserStatisticsValidator : AbstractValidator<Guid>
{
    public GetUserStatisticsValidator()
    {
        RuleFor(x => x).NotEmpty();
    }
}