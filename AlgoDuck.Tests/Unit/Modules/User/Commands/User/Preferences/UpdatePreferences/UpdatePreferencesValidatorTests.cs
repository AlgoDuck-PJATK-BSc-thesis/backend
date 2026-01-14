using AlgoDuck.Modules.User.Commands.User.Preferences.UpdatePreferences;
using AlgoDuck.Modules.User.Shared.DTOs;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Unit.Modules.User.Commands.User.Preferences.UpdatePreferences;

public sealed class UpdatePreferencesValidatorTests
{
    [Fact]
    public void Validate_WhenNoWeeklyRemindersProvided_ThenHasNoValidationErrors()
    {
        var validator = new UpdatePreferencesValidator();
        var dto = new UpdatePreferencesDto();

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenDuplicateDays_ThenHasValidationError()
    {
        var validator = new UpdatePreferencesValidator();
        var dto = new UpdatePreferencesDto
        {
            WeeklyReminders = new List<Reminder>
            {
                new() { Day = "Mon", Enabled = true, Hour = 8, Minute = 0 },
                new() { Day = "Mon", Enabled = true, Hour = 9, Minute = 0 }
            }
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.WeeklyReminders);
    }

    [Fact]
    public void Validate_WhenHourOutOfRange_ThenHasValidationError()
    {
        var validator = new UpdatePreferencesValidator();
        var dto = new UpdatePreferencesDto
        {
            WeeklyReminders = new List<Reminder>
            {
                new() { Day = "Tue", Enabled = true, Hour = 24, Minute = 0 }
            }
        };

        var result = validator.TestValidate(dto);

        Assert.True(result.Errors.Count > 0);
    }

    [Fact]
    public void Validate_WhenMinuteNotZero_ThenHasValidationError()
    {
        var validator = new UpdatePreferencesValidator();
        var dto = new UpdatePreferencesDto
        {
            WeeklyReminders = new List<Reminder>
            {
                new() { Day = "Wed", Enabled = true, Hour = 10, Minute = 30 }
            }
        };

        var result = validator.TestValidate(dto);

        Assert.True(result.Errors.Count > 0);
    }

    [Fact]
    public void Validate_WhenDayInvalid_ThenHasValidationError()
    {
        var validator = new UpdatePreferencesValidator();
        var dto = new UpdatePreferencesDto
        {
            WeeklyReminders = new List<Reminder>
            {
                new() { Day = "Monday", Enabled = true, Hour = 8, Minute = 0 }
            }
        };

        var result = validator.TestValidate(dto);

        Assert.True(result.Errors.Count > 0);
    }

    [Fact]
    public void Validate_WhenValidWeeklyReminders_ThenHasNoValidationErrors()
    {
        var validator = new UpdatePreferencesValidator();
        var dto = new UpdatePreferencesDto
        {
            WeeklyReminders = new List<Reminder>
            {
                new() { Day = "Mon", Enabled = true, Hour = 8, Minute = 0 },
                new() { Day = "Fri", Enabled = true, Hour = 21, Minute = 0 },
                new() { Day = "Sun", Enabled = false, Hour = 10, Minute = 0 }
            }
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
