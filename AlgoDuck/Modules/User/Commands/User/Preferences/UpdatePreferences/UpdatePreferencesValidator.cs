using FluentValidation;

namespace AlgoDuck.Modules.User.Commands.User.Preferences.UpdatePreferences;

public sealed class UpdatePreferencesValidator : AbstractValidator<UpdatePreferencesDto>
{
    public UpdatePreferencesValidator()
    {
        RuleFor(x => x.Language)
            .NotEmpty()
            .Must(l => l == "en" || l == "pl")
            .WithMessage("Language must be either 'en' or 'pl'.");
    }
}