using AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.UpdateCohort;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.Admin.Cohorts.UpdateCohort;

public sealed class UpdateCohortValidatorTests
{
    [Fact]
    public void Validate_WhenValid_IsValid()
    {
        var v = new UpdateCohortValidator();

        var dto = new UpdateCohortDto
        {
            Name = "Cohort A"
        };

        var result = v.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNameEmpty_IsInvalid()
    {
        var v = new UpdateCohortValidator();

        var dto = new UpdateCohortDto
        {
            Name = ""
        };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WhenNameTooLong_IsInvalid()
    {
        var v = new UpdateCohortValidator();

        var dto = new UpdateCohortDto
        {
            Name = new string('a', 257)
        };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }
}