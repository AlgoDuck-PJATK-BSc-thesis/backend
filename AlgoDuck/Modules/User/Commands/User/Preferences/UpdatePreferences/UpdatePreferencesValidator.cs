using AlgoDuck.Modules.User.Shared.DTOs;
using FluentValidation;

namespace AlgoDuck.Modules.User.Commands.User.Preferences.UpdatePreferences;

public sealed class UpdatePreferencesValidator : AbstractValidator<UpdatePreferencesDto>
{
    private static readonly HashSet<string> ValidDays = new() { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

    public UpdatePreferencesValidator()
    {

        When(x => x.WeeklyReminders is not null, () =>
        {
            RuleFor(x => x.WeeklyReminders!)
                .Must(NoDuplicateDays)
                .WithMessage("WeeklyReminders must not contain duplicate days.");

            RuleForEach(x => x.WeeklyReminders!)
                .SetValidator(new ReminderValidator());
        });
    }

    private static bool NoDuplicateDays(List<Reminder> reminders)
    {
        var seen = new HashSet<string>();
        foreach (var r in reminders)
        {
            
            if (!ValidDays.Contains(r.Day))
            {
                return false;
            }

            if (!seen.Add(r.Day))
            {
                return false;
            }
        }

        return true;
    }

    private sealed class ReminderValidator : AbstractValidator<Reminder>
    {
        public ReminderValidator()
        {
            RuleFor(r => r.Day)
                .NotEmpty()
                .Must(d => ValidDays.Contains(d));

            RuleFor(r => r.Hour)
                .InclusiveBetween(0, 23);

            RuleFor(r => r.Minute)
                .Equal(0);

            RuleFor(r => r.Enabled)
                .NotNull();
        }
    }
}
