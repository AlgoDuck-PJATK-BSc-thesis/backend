using AlgoDuck.Modules.User.Queries.User.Stats.GetUserStatistics;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.User.Stats.GetUserStatistics;

public sealed class GetUserStatisticsValidatorTests
{
    [Fact]
    public void Validate_WhenUserIdEmpty_ThenHasValidationError()
    {
        var validator = new GetUserStatisticsValidator();

        var result = validator.TestValidate(Guid.Empty);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WhenUserIdValid_ThenHasNoValidationErrors()
    {
        var validator = new GetUserStatisticsValidator();

        var result = validator.TestValidate(Guid.NewGuid());

        result.ShouldNotHaveAnyValidationErrors();
    }
}