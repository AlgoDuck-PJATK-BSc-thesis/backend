using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Queries.User.Settings.GetUserConfig;

public sealed class GetUserConfigHandler : IGetUserConfigHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ApplicationCommandDbContext _dbContext;

    public GetUserConfigHandler(IUserRepository userRepository, ApplicationCommandDbContext dbContext)
    {
        _userRepository = userRepository;
        _dbContext = dbContext;
    }

    public async Task<UserConfigDto> HandleAsync(GetUserConfigRequestDto query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            throw new UserNotFoundException(query.UserId);
        }

        var config = user.UserConfig;
        var reminders = await BuildWeeklyRemindersAsync(user.Id, cancellationToken);

        return new UserConfigDto
        {
            IsDarkMode = config?.IsDarkMode ?? false,
            IsHighContrast = config?.IsHighContrast ?? false,
            EmailNotificationsEnabled = config?.EmailNotificationsEnabled ?? false,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            WeeklyReminders = reminders,
            S3AvatarUrl = string.Empty
        };
    }

    private async Task<List<Reminder>> BuildWeeklyRemindersAsync(Guid userId, CancellationToken cancellationToken)
    {
        const int defaultsHour = 8;

        var joinSet = _dbContext.Set<Models.UserSetStudyReminder>();

        var rows = await joinSet
            .Where(e => e.UserId == userId)
            .Select(e => new
            {
                e.StudyReminderId,
                e.Hour
            })
            .ToListAsync(cancellationToken);

        var byId = rows.ToDictionary(x => x.StudyReminderId, x => x.Hour);

        int? mon = byId.TryGetValue(1, out var h1) ? h1 : null;
        int? tue = byId.TryGetValue(2, out var h2) ? h2 : null;
        int? wed = byId.TryGetValue(3, out var h3) ? h3 : null;
        int? thu = byId.TryGetValue(4, out var h4) ? h4 : null;
        int? fri = byId.TryGetValue(5, out var h5) ? h5 : null;
        int? sat = byId.TryGetValue(6, out var h6) ? h6 : null;
        int? sun = byId.TryGetValue(7, out var h7) ? h7 : null;

        return new List<Reminder>
        {
            new Reminder { Day = "Mon", Enabled = mon.HasValue, Hour = mon ?? defaultsHour, Minute = 0 },
            new Reminder { Day = "Tue", Enabled = tue.HasValue, Hour = tue ?? defaultsHour, Minute = 0 },
            new Reminder { Day = "Wed", Enabled = wed.HasValue, Hour = wed ?? defaultsHour, Minute = 0 },
            new Reminder { Day = "Thu", Enabled = thu.HasValue, Hour = thu ?? defaultsHour, Minute = 0 },
            new Reminder { Day = "Fri", Enabled = fri.HasValue, Hour = fri ?? defaultsHour, Minute = 0 },
            new Reminder { Day = "Sat", Enabled = sat.HasValue, Hour = sat ?? defaultsHour, Minute = 0 },
            new Reminder { Day = "Sun", Enabled = sun.HasValue, Hour = sun ?? defaultsHour, Minute = 0 }
        };
    }
}
