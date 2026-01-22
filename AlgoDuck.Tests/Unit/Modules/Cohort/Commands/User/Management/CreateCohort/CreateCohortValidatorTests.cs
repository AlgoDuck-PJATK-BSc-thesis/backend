using AlgoDuck.Modules.Cohort.Commands.User.Management.CreateCohort;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.User.Management.CreateCohort;

public class CreateCohortValidatorTests
{
    [Fact]
    public void Validate_WhenNameIsEmpty_ThenHasValidationErrorWithExpectedMessage()
    {
        var validator = new CreateCohortValidator();
        var dto = new CreateCohortDto
        {
            Name = ""
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Cohort's name is required.");
    }

    [Fact]
    public void Validate_WhenNameIsWhitespace_ThenHasValidationErrorWithExpectedMessage()
    {
        var validator = new CreateCohortValidator();
        var dto = new CreateCohortDto
        {
            Name = "   "
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Cohort's name is required.");
    }

    [Fact]
    public void Validate_WhenNameIsTooShort_ThenHasValidationErrorWithExpectedMessage()
    {
        var validator = new CreateCohortValidator();
        var dto = new CreateCohortDto
        {
            Name = "ab"
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("The length of Cohort's name must be at least 3 characters. You entered 2 characters.");
    }

    [Fact]
    public void Validate_WhenNameIsTooLong_ThenHasValidationErrorWithExpectedMessage()
    {
        var validator = new CreateCohortValidator();
        var dto = new CreateCohortDto
        {
            Name = new string('a', 257)
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("The length of Cohort's name must be 256 characters or fewer. You entered 257 characters.");
    }

    [Fact]
    public void Validate_WhenNameIsValid_ThenHasNoValidationErrors()
    {
        var validator = new CreateCohortValidator();
        var dto = new CreateCohortDto
        {
            Name = "My Cohort"
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenNameIsValidAfterTrimming_ThenHasNoValidationErrors()
    {
        var validator = new CreateCohortValidator();
        var dto = new CreateCohortDto
        {
            Name = "  My Cohort  "
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
