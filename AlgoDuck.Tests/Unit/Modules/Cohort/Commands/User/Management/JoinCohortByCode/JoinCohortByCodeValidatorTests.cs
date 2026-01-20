using AlgoDuck.Modules.Cohort.Commands.User.Management.JoinCohortByCode;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.User.Management.JoinCohortByCode;

public sealed class JoinCohortByCodeValidatorTests
{
    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var validator = new JoinCohortByCodeValidator();
        var dto = new JoinCohortByCodeDto { Code = "ABC123" };

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenEmpty_Fails()
    {
        var validator = new JoinCohortByCodeValidator();
        var dto = new JoinCohortByCodeDto { Code = "" };

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenTooLong_Fails()
    {
        var validator = new JoinCohortByCodeValidator();
        var dto = new JoinCohortByCodeDto { Code = new string('A', 17) };

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenContainsNonAlphanumeric_Fails()
    {
        var validator = new JoinCohortByCodeValidator();
        var dto = new JoinCohortByCodeDto { Code = "ABC-123" };

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenLowercase_Fails()
    {
        var validator = new JoinCohortByCodeValidator();
        var dto = new JoinCohortByCodeDto { Code = "abc123" };

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }
}