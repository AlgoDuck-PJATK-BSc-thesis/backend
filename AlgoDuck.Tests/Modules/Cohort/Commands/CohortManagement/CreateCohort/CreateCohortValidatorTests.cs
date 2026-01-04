using AlgoDuck.Modules.Cohort.Commands.User.Management.CreateCohort;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.Cohort.Commands.CohortManagement.CreateCohort;

public class CreateCohortValidatorTests
{
    [Fact]
    public void Validate_WhenNameIsEmpty_ThenHasValidationError()
    {
        var validator = new CreateCohortValidator();
        var dto = new CreateCohortDto
        {
            Name = ""
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WhenNameIsTooShort_ThenHasValidationError()
    {
        var validator = new CreateCohortValidator();
        var dto = new CreateCohortDto
        {
            Name = "ab"
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WhenNameIsTooLong_ThenHasValidationError()
    {
        var validator = new CreateCohortValidator();
        var dto = new CreateCohortDto
        {
            Name = new string('a', 101)
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
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
}