using FluentValidation;

namespace AlgoDuck.Modules.User.Commands.User.Preferences.SetEditorTheme;

public sealed class SetEditorThemeValidator : AbstractValidator<SetEditorThemeDto>
{
    public SetEditorThemeValidator()
    {
        RuleFor(x => x.EditorThemeId).NotEmpty();
    }
}