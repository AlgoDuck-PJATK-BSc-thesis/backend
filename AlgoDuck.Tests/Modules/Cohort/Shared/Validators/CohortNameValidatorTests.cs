using AlgoDuck.Modules.Cohort.Shared.Validators;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Validators;

public sealed class CohortNameValidatorTests
{
    private readonly CohortNameValidator _validator = new();

    [Fact]
    public void Validate_WhenNameIsEmpty_ThenHasValidationError()
    {
        var name = string.Empty;

        var result = _validator.TestValidate(name);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WhenNameIsTooShort_ThenHasValidationError()
    {
        var name = "ab";

        var result = _validator.TestValidate(name);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WhenNameIsTooLong_ThenHasValidationError()
    {
        var name = new string('x', 65);

        var result = _validator.TestValidate(name);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WhenNameLengthIsWithinRange_ThenHasNoValidationError()
    {
        var name = "My Cohort";

        var result = _validator.TestValidate(name);

        result.ShouldNotHaveValidationErrorFor(x => x);
    }
}