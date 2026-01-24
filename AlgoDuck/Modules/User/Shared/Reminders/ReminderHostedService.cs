using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Shared.Reminders;

public sealed class ReminderHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;

    public ReminderHostedService(IServiceScopeFactory scopeFactory, IHostEnvironment environment)
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_environment.IsEnvironment("Testing"))
        {
            return;
        }

        var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await TickAsync(stoppingToken);
        }
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationCommandDbContext>();
        var calculator = scope.ServiceProvider.GetRequiredService<ReminderNextAtCalculator>();
        var sender = scope.ServiceProvider.GetRequiredService<IReminderEmailSender>();

        var nowUtc = DateTimeOffset.UtcNow;

        var dueConfigs = await db.UserConfigs
            .Include(c => c.User)
            .Where(c => c.EmailNotificationsEnabled && c.StudyReminderNextAtUtc != null && c.StudyReminderNextAtUtc <= nowUtc)
            .ToListAsync(cancellationToken);

        foreach (var cfg in dueConfigs)
        {
            var user = cfg.User;

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                continue;
            }

            try
            {
                await sender.SendStudyReminderAsync(user.Id, user.Email!, cancellationToken);
            }
            catch
            {
                continue;
            }

            var schedule = await LoadEnabledScheduleAsync(db, user.Id, cancellationToken);
            cfg.StudyReminderNextAtUtc = calculator.ComputeNextAtUtc(cfg, schedule, nowUtc, true);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task<Dictionary<DayOfWeek, int>> LoadEnabledScheduleAsync(ApplicationCommandDbContext db, Guid userId, CancellationToken cancellationToken)
    {
        var joinSet = db.Set<UserSetStudyReminder>();

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
