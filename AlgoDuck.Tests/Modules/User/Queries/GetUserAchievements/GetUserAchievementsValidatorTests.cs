using AlgoDuck.Modules.User.Queries.GetUserAchievements;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.User.Queries.GetUserAchievements;

public sealed class GetUserAchievementsValidatorTests
{
    [Fact]
    public void Validate_WhenPageLessThan1_ThenHasValidationError()
    {
        var validator = new GetUserAchievementsValidator();

        var result = validator.TestValidate(new GetUserAchievementsRequestDto
        {
            Page = 0,
            PageSize = 20
        });

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Validate_WhenPageSizeLessThan1_ThenHasValidationError()
    {
        var validator = new GetUserAchievementsValidator();

        var result = validator.TestValidate(new GetUserAchievementsRequestDto
        {
            Page = 1,
            PageSize = 0
        });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WhenPageSizeGreaterThan100_ThenHasValidationError()
    {
        var validator = new GetUserAchievementsValidator();

        var result = validator.TestValidate(new GetUserAchievementsRequestDto
        {
            Page = 1,
            PageSize = 101
        });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WhenCodeFilterTooLong_ThenHasValidationError()
    {
        var validator = new GetUserAchievementsValidator();

        var result = validator.TestValidate(new GetUserAchievementsRequestDto
        {
            Page = 1,
            PageSize = 20,
            CodeFilter = new string('a', 65)
        });

        result.ShouldHaveValidationErrorFor(x => x.CodeFilter);
    }

    [Fact]
    public void Validate_WhenValid_ThenHasNoValidationErrors()
    {
        var validator = new GetUserAchievementsValidator();

        var result = validator.TestValidate(new GetUserAchievementsRequestDto
        {
            Page = 1,
            PageSize = 20,
            CodeFilter = "ACH",
            Completed = true
        });

        result.ShouldNotHaveAnyValidationErrors();
    }
}