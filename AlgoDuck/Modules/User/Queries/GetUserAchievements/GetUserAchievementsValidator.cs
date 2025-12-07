using AlgoDuck.Modules.User.Shared.Constants;
using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.GetUserAchievements;

public sealed class GetUserAchievementsValidator : AbstractValidator<GetUserAchievementsRequestDto>
{
    public GetUserAchievementsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(UserConstants.AchievementsDefaultPage);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(UserConstants.AchievementsMinPageSize)
            .LessThanOrEqualTo(UserConstants.AchievementsMaxPageSize);

        RuleFor(x => x.CodeFilter)
            .MaximumLength(64);
    }
}