using AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.CreateCohort;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.Admin.Cohorts.CreateCohort;

public sealed class CreateCohortValidatorTests
{
    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var validator = new CreateCohortValidator();
        var dto = new CreateCohortDto { Name = "Cohort A" };

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenEmpty_Fails()
    {
        var validator = new CreateCohortValidator();
        var dto = new CreateCohortDto { Name = "" };

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenTooLong_Fails()
    {
        var validator = new CreateCohortValidator();
        var dto = new CreateCohortDto { Name = new string('A', 257) };

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }
}