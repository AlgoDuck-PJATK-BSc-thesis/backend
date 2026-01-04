using AlgoDuck.Modules.User.Queries.User.Profile.SearchUsers;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.User.Queries.SearchUsers;

public sealed class SearchUsersValidatorTests
{
    [Fact]
    public void Validate_WhenPageLessThan1_ThenHasValidationError()
    {
        var validator = new SearchUsersValidator();

        var result = validator.TestValidate(new SearchUsersDto
        {
            Query = "a",
            Page = 0,
            PageSize = 20
        });

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Validate_WhenPageSizeLessThan1_ThenHasValidationError()
    {
        var validator = new SearchUsersValidator();

        var result = validator.TestValidate(new SearchUsersDto
        {
            Query = "a",
            Page = 1,
            PageSize = 0
        });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WhenPageSizeGreaterThan100_ThenHasValidationError()
    {
        var validator = new SearchUsersValidator();

        var result = validator.TestValidate(new SearchUsersDto
        {
            Query = "a",
            Page = 1,
            PageSize = 101
        });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WhenQueryTooLong_ThenHasValidationError()
    {
        var validator = new SearchUsersValidator();

        var result = validator.TestValidate(new SearchUsersDto
        {
            Query = new string('a', 257),
            Page = 1,
            PageSize = 20
        });

        result.ShouldHaveValidationErrorFor(x => x.Query);
    }

    [Fact]
    public void Validate_WhenValid_ThenHasNoValidationErrors()
    {
        var validator = new SearchUsersValidator();

        var result = validator.TestValidate(new SearchUsersDto
        {
            Query = "abc",
            Page = 1,
            PageSize = 20
        });

        result.ShouldNotHaveAnyValidationErrors();
    }
}