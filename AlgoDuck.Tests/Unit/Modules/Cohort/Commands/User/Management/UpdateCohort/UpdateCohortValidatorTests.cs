using AlgoDuck.Modules.Cohort.Commands.User.Management.UpdateCohort;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.User.Management.UpdateCohort;

public class UpdateCohortValidatorTests
{
    [Fact]
    public void Validate_WhenNameEmpty_ThenHasValidationErrorWithExpectedMessage()
    {
        var validator = new UpdateCohortValidator();
        var dto = new UpdateCohortDto
        {
            Name = ""
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Cohort's name is required.");
    }

    [Fact]
    public void Validate_WhenNameWhitespace_ThenHasValidationErrorWithExpectedMessage()
    {
        var validator = new UpdateCohortValidator();
        var dto = new UpdateCohortDto
        {
            Name = "   "
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Cohort's name is required.");
    }

    [Fact]
    public void Validate_WhenNameTooShort_ThenHasValidationErrorWithExpectedMessage()
    {
        var validator = new UpdateCohortValidator();
        var dto = new UpdateCohortDto
        {
            Name = "ab"
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("The length of Cohort's name must be at least 3 characters. You entered 2 characters.");
    }

    [Fact]
    public void Validate_WhenNameTooLong_ThenHasValidationErrorWithExpectedMessage()
    {
        var validator = new UpdateCohortValidator();
        var dto = new UpdateCohortDto
        {
            Name = new string('a', 257)
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("The length of Cohort's name must be 256 characters or fewer. You entered 257 characters.");
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

    [Fact]
    public void Validate_WhenNameValidAfterTrimming_ThenHasNoValidationErrors()
    {
        var validator = new UpdateCohortValidator();
        var dto = new UpdateCohortDto
        {
            Name = "  Updated Cohort Name  "
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
