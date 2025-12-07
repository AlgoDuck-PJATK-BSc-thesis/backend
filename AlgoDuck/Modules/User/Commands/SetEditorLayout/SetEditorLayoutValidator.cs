using FluentValidation;

namespace AlgoDuck.Modules.User.Commands.SetEditorLayout;

public sealed class SetEditorLayoutValidator : AbstractValidator<SetEditorLayoutDto>
{
    public SetEditorLayoutValidator()
    {
        RuleFor(x => x.EditorThemeId).NotEmpty();
    }
}