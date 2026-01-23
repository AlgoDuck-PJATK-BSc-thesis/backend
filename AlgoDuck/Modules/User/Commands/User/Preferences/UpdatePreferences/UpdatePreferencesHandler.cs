using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Reminders;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Commands.User.Preferences.UpdatePreferences;

public sealed class UpdatePreferencesHandler : IUpdatePreferencesHandler
{
    private readonly ApplicationCommandDbContext _dbContext;
    private readonly IValidator<UpdatePreferencesDto> _validator;
    private readonly ReminderNextAtCalculator _calculator;

    private static readonly IReadOnlyDictionary<string, int> DayToStudyReminderId = new Dictionary<string, int>
    {
        ["Mon"] = 1,
        ["Tue"] = 2,
        ["Wed"] = 3,
        ["Thu"] = 4,
        ["Fri"] = 5,
        ["Sat"] = 6,
        ["Sun"] = 7
    };

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
            config = new UserConfig
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

        Dictionary<DayOfWeek, int> schedule;

        if (remindersProvided)
        {
            schedule = await ApplyWeeklyRemindersAsync(userId, dto.WeeklyReminders!, cancellationToken);
        }
        else
        {
            schedule = await LoadEnabledScheduleAsync(userId, cancellationToken);
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
                config.StudyReminderNextAtUtc = _calculator.ComputeNextAtUtc(config, schedule, nowUtc, false);
            }
            else
            {
                if (config.StudyReminderNextAtUtc.HasValue && config.StudyReminderNextAtUtc.Value <= nowUtc)
                {
                    config.StudyReminderNextAtUtc = _calculator.ComputeNextAtUtc(config, schedule, nowUtc, false);
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Dictionary<DayOfWeek, int>> ApplyWeeklyRemindersAsync(Guid userId, List<Reminder> reminders, CancellationToken cancellationToken)
    {
        var inputByDay = reminders.ToDictionary(r => r.Day, StringComparer.Ordinal);

        var joinSet = _dbContext.Set<UserSetStudyReminder>();

        var existingRows = await joinSet
            .Where(e => e.UserId == userId)
            .ToListAsync(cancellationToken);

        var existingById = existingRows.ToDictionary(e => e.StudyReminderId);

        var enabledSchedule = new Dictionary<DayOfWeek, int>();

        foreach (var kvp in DayToStudyReminderId)
        {
            var dayKey = kvp.Key;
            var studyReminderId = kvp.Value;

            int? hour = null;

            if (inputByDay.TryGetValue(dayKey, out var r) && r.Enabled)
            {
                hour = r.Hour;
                enabledSchedule[StudyReminderIdToDayOfWeek(studyReminderId)] = r.Hour;
            }

            if (existingById.TryGetValue(studyReminderId, out var row))
            {
                row.Hour = hour;
            }
            else
            {
                joinSet.Add(new UserSetStudyReminder
                {
                    UserId = userId,
                    StudyReminderId = studyReminderId,
                    Hour = hour
                });
            }
        }

        return enabledSchedule;
    }

    private async Task<Dictionary<DayOfWeek, int>> LoadEnabledScheduleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var joinSet = _dbContext.Set<UserSetStudyReminder>();

        var rows = await joinSet
            .Where(e => e.UserId == userId && e.Hour != null)
            .Select(e => new
            {
                e.StudyReminderId,
                e.Hour
            })
            .ToListAsync(cancellationToken);

        var schedule = new Dictionary<DayOfWeek, int>();

        foreach (var r in rows)
        {
            if (!r.Hour.HasValue)
            {
                continue;
            }

            schedule[StudyReminderIdToDayOfWeek(r.StudyReminderId)] = r.Hour.Value;
        }

        return schedule;
    }

    private static DayOfWeek StudyReminderIdToDayOfWeek(int studyReminderId)
    {
        return studyReminderId switch
        {
            1 => DayOfWeek.Monday,
            2 => DayOfWeek.Tuesday,
            3 => DayOfWeek.Wednesday,
            4 => DayOfWeek.Thursday,
            5 => DayOfWeek.Friday,
            6 => DayOfWeek.Saturday,
            7 => DayOfWeek.Sunday,
            _ => DayOfWeek.Monday
        };
    }
}
