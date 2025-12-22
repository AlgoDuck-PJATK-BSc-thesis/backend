using AlgoDuck.Modules.User.Commands.SetEditorLayout;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.User.Commands.SetEditorLayout;

public sealed class SetEditorLayoutValidatorTests
{
    [Fact]
    public void Validate_WhenEditorThemeIdEmpty_ThenHasValidationError()
    {
        var validator = new SetEditorLayoutValidator();
        var dto = new SetEditorLayoutDto
        {
            EditorThemeId = Guid.Empty
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EditorThemeId);
    }

    [Fact]
    public void Validate_WhenValid_ThenHasNoValidationErrors()
    {
        var validator = new SetEditorLayoutValidator();
        var dto = new SetEditorLayoutDto
        {
            EditorThemeId = Guid.NewGuid()
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}