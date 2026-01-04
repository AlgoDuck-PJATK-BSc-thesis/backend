using AlgoDuck.Modules.User.Queries.User.Stats.GetUserSolvedProblems;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.User.Queries.GetUserSolvedProblems;

public sealed class GetUserSolvedProblemsValidatorTests
{
    [Fact]
    public void Validate_WhenPageLessThan1_ThenHasValidationError()
    {
        var validator = new GetUserSolvedProblemsValidator();

        var result = validator.TestValidate(new GetUserSolvedProblemsQuery
        {
            Page = 0,
            PageSize = 50
        });

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Validate_WhenPageSizeLessThan1_ThenHasValidationError()
    {
        var validator = new GetUserSolvedProblemsValidator();

        var result = validator.TestValidate(new GetUserSolvedProblemsQuery
        {
            Page = 1,
            PageSize = 0
        });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WhenPageSizeGreaterThan100_ThenHasValidationError()
    {
        var validator = new GetUserSolvedProblemsValidator();

        var result = validator.TestValidate(new GetUserSolvedProblemsQuery
        {
            Page = 1,
            PageSize = 101
        });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WhenValid_ThenHasNoValidationErrors()
    {
        var validator = new GetUserSolvedProblemsValidator();

        var result = validator.TestValidate(new GetUserSolvedProblemsQuery
        {
            Page = 1,
            PageSize = 50
        });

        result.ShouldNotHaveAnyValidationErrors();
    }
}