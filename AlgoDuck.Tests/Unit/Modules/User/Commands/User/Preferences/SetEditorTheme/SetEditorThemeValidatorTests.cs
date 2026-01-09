using AlgoDuck.Modules.User.Commands.User.Preferences.SetEditorTheme;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Unit.Modules.User.Commands.User.Preferences.SetEditorTheme;

public sealed class SetEditorThemeValidatorTests
{
    [Fact]
    public void Validate_WhenEditorThemeIdEmpty_ThenHasValidationError()
    {
        var validator = new SetEditorThemeValidator();
        var dto = new SetEditorThemeDto
        {
            EditorThemeId = Guid.Empty
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EditorThemeId);
    }

    [Fact]
    public void Validate_WhenValid_ThenHasNoValidationErrors()
    {
        var validator = new SetEditorThemeValidator();
        var dto = new SetEditorThemeDto
        {
            EditorThemeId = Guid.NewGuid()
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}