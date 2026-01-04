using FluentValidation;

namespace AlgoDuck.Modules.User.Commands.User.Preferences.SetEditorLayout;

public sealed class SetEditorLayoutValidator : AbstractValidator<SetEditorLayoutDto>
{
    public SetEditorLayoutValidator()
    {
        RuleFor(x => x.EditorThemeId).NotEmpty();
    }
}