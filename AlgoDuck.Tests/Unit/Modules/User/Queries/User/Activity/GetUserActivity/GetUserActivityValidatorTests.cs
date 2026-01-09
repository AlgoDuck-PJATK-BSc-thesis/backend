using AlgoDuck.Modules.User.Queries.User.Activity.GetUserActivity;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.User.Activity.GetUserActivity;

public sealed class GetUserActivityValidatorTests
{
    [Fact]
    public void Validate_WhenPageLessThan1_ThenHasValidationError()
    {
        var validator = new GetUserActivityValidator();

        var result = validator.TestValidate(new GetUserActivityRequestDto
        {
            Page = 0,
            PageSize = 20
        });

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Validate_WhenPageSizeLessThan1_ThenHasValidationError()
    {
        var validator = new GetUserActivityValidator();

        var result = validator.TestValidate(new GetUserActivityRequestDto
        {
            Page = 1,
            PageSize = 0
        });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WhenPageSizeGreaterThan100_ThenHasValidationError()
    {
        var validator = new GetUserActivityValidator();

        var result = validator.TestValidate(new GetUserActivityRequestDto
        {
            Page = 1,
            PageSize = 101
        });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WhenValid_ThenHasNoValidationErrors()
    {
        var validator = new GetUserActivityValidator();

        var result = validator.TestValidate(new GetUserActivityRequestDto
        {
            Page = 1,
            PageSize = 20
        });

        result.ShouldNotHaveAnyValidationErrors();
    }
}