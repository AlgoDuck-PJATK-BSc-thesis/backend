
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.User.Settings.GetUserConfig;

public sealed class GetUserConfigHandler : IGetUserConfigHandler
{
    private readonly IUserRepository _userRepository;

    public GetUserConfigHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserConfigDto> HandleAsync(GetUserConfigRequestDto query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            throw new UserNotFoundException(query.UserId);
        }

        var config = user.UserConfig;

        var reminders = BuildWeeklyReminders(config);

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

    private static List<Reminder> BuildWeeklyReminders(Models.UserConfig? config)
    {
        var defaultsHour = 8;

        var mon = config?.ReminderMonHour;
        var tue = config?.ReminderTueHour;
        var wed = config?.ReminderWedHour;
        var thu = config?.ReminderThuHour;
        var fri = config?.ReminderFriHour;
        var sat = config?.ReminderSatHour;
        var sun = config?.ReminderSunHour;

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