using AlgoDuck.Modules.Cohort.Commands.User.Management.UpdateCohort;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.User.Management.UpdateCohort;

public class UpdateCohortValidatorTests
{
    [Fact]
    public void Validate_WhenNameEmpty_ThenHasValidationError()
    {
        var validator = new UpdateCohortValidator();
        var dto = new UpdateCohortDto
        {
            Name = ""
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WhenNameTooShort_ThenHasValidationError()
    {
        var validator = new UpdateCohortValidator();
        var dto = new UpdateCohortDto
        {
            Name = "ab"
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WhenNameTooLong_ThenHasValidationError()
    {
        var validator = new UpdateCohortValidator();
        var dto = new UpdateCohortDto
        {
            Name = new string('a', 101)
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WhenNameValid_ThenHasNoValidationErrors()
    {
        var validator = new UpdateCohortValidator();
        var dto = new UpdateCohortDto
        {
            Name = "Updated Cohort Name"
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}