using AlgoDuck.Modules.Cohort.Shared.Validators;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Shared.Validators;

public sealed class CohortNameValidatorTests
{
    private readonly CohortNameValidator _validator = new();

    [Fact]
    public void Validate_WhenNameIsEmpty_ThenHasValidationErrorWithExpectedMessage()
    {
        var name = string.Empty;

        var result = _validator.TestValidate(name);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Cohort's name is required.");
    }

    [Fact]
    public void Validate_WhenNameIsWhitespace_ThenHasValidationErrorWithExpectedMessage()
    {
        var name = "   ";

        var result = _validator.TestValidate(name);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Cohort's name is required.");
    }

    [Fact]
    public void Validate_WhenNameIsTooShort_ThenHasValidationErrorWithExpectedMessage()
    {
        var name = "ab";

        var result = _validator.TestValidate(name);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("The length of Cohort's name must be at least 3 characters. You entered 2 characters.");
    }

    [Fact]
    public void Validate_WhenNameIsTooLong_ThenHasValidationErrorWithExpectedMessage()
    {
        var name = new string('x', 257);

        var result = _validator.TestValidate(name);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("The length of Cohort's name must be 256 characters or fewer. You entered 257 characters.");
    }

    [Fact]
    public void Validate_WhenNameIsTrimmedAndWithinRange_ThenHasNoValidationError()
    {
        var name = "  My Cohort  ";

        var result = _validator.TestValidate(name);

        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WhenNameLengthIsWithinRange_ThenHasNoValidationError()
    {
        var name = "My Cohort";

        var result = _validator.TestValidate(name);

        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WhenNameLengthIsExactlyMax_ThenHasNoValidationError()
    {
        var name = new string('x', 256);

        var result = _validator.TestValidate(name);

        result.ShouldNotHaveValidationErrorFor(x => x);
    }
}
