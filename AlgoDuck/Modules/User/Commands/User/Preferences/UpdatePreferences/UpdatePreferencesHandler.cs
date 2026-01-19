using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Reminders;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Commands.User.Preferences.UpdatePreferences.UpdatePreferences;

public sealed class UpdatePreferencesHandler : IUpdatePreferencesHandler
{
    private readonly ApplicationCommandDbContext _dbContext;
    private readonly IValidator<UpdatePreferencesDto> _validator;
    private readonly ReminderNextAtCalculator _calculator;

    public UpdatePreferencesHandler(
        ApplicationCommandDbContext dbContext,
        IValidator<UpdatePreferencesDto> validator,
        ReminderNextAtCalculator calculator)
    {
        _dbContext = dbContext;
        _validator = validator;
        _calculator = calculator;
    }

    public async Task HandleAsync(Guid userId, UpdatePreferencesDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        if (userId == Guid.Empty)
        {
            throw new Shared.Exceptions.ValidationException("User identifier is invalid.");
        }

        var userExists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == userId, cancellationToken);

        if (!userExists)
        {
            throw new UserNotFoundException("User not found.");
        }

        var config = await _dbContext.UserConfigs
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (config is null)
        {
            config = new Models.UserConfig
            {
                UserId = userId,
                EditorFontSize = 11,
                EmailNotificationsEnabled = false,
                IsDarkMode = true,
                IsHighContrast = false
            };

            _dbContext.UserConfigs.Add(config);
        }

        var emailBefore = config.EmailNotificationsEnabled;

        config.IsDarkMode = dto.IsDarkMode;
        config.IsHighContrast = dto.IsHighContrast;
        config.EmailNotificationsEnabled = dto.EmailNotificationsEnabled;

        var remindersProvided = dto.WeeklyReminders is not null;

        if (remindersProvided)
        {
            ApplyWeeklyReminders(config, dto);
        }

        var nowUtc = DateTimeOffset.UtcNow;

        if (!config.EmailNotificationsEnabled)
        {
            config.StudyReminderNextAtUtc = null;
        }
        else
        {
            var shouldRecompute = remindersProvided || emailBefore != config.EmailNotificationsEnabled;

            if (shouldRecompute)
            {
                config.StudyReminderNextAtUtc = _calculator.ComputeNextAtUtc(config, nowUtc, false);
            }
            else
            {
                if (config.StudyReminderNextAtUtc.HasValue && config.StudyReminderNextAtUtc.Value <= nowUtc)
                {
                    config.StudyReminderNextAtUtc = _calculator.ComputeNextAtUtc(config, nowUtc, false);
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyWeeklyReminders(Models.UserConfig config, UpdatePreferencesDto dto)
    {
        config.ReminderMonHour = null;
        config.ReminderTueHour = null;
        config.ReminderWedHour = null;
        config.ReminderThuHour = null;
        config.ReminderFriHour = null;
        config.ReminderSatHour = null;
        config.ReminderSunHour = null;

        foreach (var r in dto.WeeklyReminders!)
        {
            var hour = r.Enabled ? r.Hour : (int?)null;

            switch (r.Day)
            {
                case "Mon":
                    config.ReminderMonHour = hour;
                    break;
                case "Tue":
                    config.ReminderTueHour = hour;
                    break;
                case "Wed":
                    config.ReminderWedHour = hour;
                    break;
                case "Thu":
                    config.ReminderThuHour = hour;
                    break;
                case "Fri":
                    config.ReminderFriHour = hour;
                    break;
                case "Sat":
                    config.ReminderSatHour = hour;
                    break;
                case "Sun":
                    config.ReminderSunHour = hour;
                    break;
            }
        }
    }
}
