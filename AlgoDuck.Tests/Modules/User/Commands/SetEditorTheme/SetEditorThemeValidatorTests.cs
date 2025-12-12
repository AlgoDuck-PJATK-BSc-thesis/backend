using AlgoDuck.Modules.User.Commands.SetEditorTheme;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.User.Commands.SetEditorTheme;

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