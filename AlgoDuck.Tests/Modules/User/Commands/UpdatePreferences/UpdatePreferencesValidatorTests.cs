using AlgoDuck.Modules.User.Commands.User.Preferences.UpdatePreferences;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.User.Commands.UpdatePreferences;

public sealed class UpdatePreferencesValidatorTests
{
    [Fact]
    public void Validate_WhenLanguageEmpty_ThenHasValidationError()
    {
        var validator = new UpdatePreferencesValidator();
        var dto = new UpdatePreferencesDto
        {
            Language = ""
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Language);
    }

    [Fact]
    public void Validate_WhenLanguageInvalid_ThenHasValidationError()
    {
        var validator = new UpdatePreferencesValidator();
        var dto = new UpdatePreferencesDto
        {
            Language = "de"
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Language);
    }

    [Fact]
    public void Validate_WhenLanguageValidEn_ThenHasNoValidationErrors()
    {
        var validator = new UpdatePreferencesValidator();
        var dto = new UpdatePreferencesDto
        {
            Language = "en"
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenLanguageValidPl_ThenHasNoValidationErrors()
    {
        var validator = new UpdatePreferencesValidator();
        var dto = new UpdatePreferencesDto
        {
            Language = "pl"
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}