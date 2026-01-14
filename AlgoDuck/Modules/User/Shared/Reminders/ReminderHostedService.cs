using AlgoDuck.DAL;
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

            cfg.StudyReminderNextAtUtc = calculator.ComputeNextAtUtc(cfg, nowUtc, true);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
