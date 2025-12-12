using AlgoDuck.Modules.User.Queries.GetUserRankings;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.User.Queries.GetUserRankings;

public sealed class GetUserRankingsValidatorTests
{
    [Fact]
    public void Validate_WhenPageLessThan1_ThenHasValidationError()
    {
        var validator = new GetUserRankingsValidator();

        var result = validator.TestValidate(new GetUserRankingsQuery
        {
            Page = 0,
            PageSize = 20
        });

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Validate_WhenPageSizeLessThan1_ThenHasValidationError()
    {
        var validator = new GetUserRankingsValidator();

        var result = validator.TestValidate(new GetUserRankingsQuery
        {
            Page = 1,
            PageSize = 0
        });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WhenPageSizeGreaterThan100_ThenHasValidationError()
    {
        var validator = new GetUserRankingsValidator();

        var result = validator.TestValidate(new GetUserRankingsQuery
        {
            Page = 1,
            PageSize = 101
        });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WhenValid_ThenHasNoValidationErrors()
    {
        var validator = new GetUserRankingsValidator();

        var result = validator.TestValidate(new GetUserRankingsQuery
        {
            Page = 1,
            PageSize = 20
        });

        result.ShouldNotHaveAnyValidationErrors();
    }
}