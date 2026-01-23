using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Utils;

namespace AlgoDuck.Modules.User.Shared.Reminders;

public sealed class ReminderNextAtCalculator
{
    public DateTimeOffset? ComputeNextAtUtc(UserConfig config, Dictionary<DayOfWeek, int> schedule, DateTimeOffset nowUtc, bool justSent)
    {
        if (!config.EmailNotificationsEnabled)
        {
            return null;
        }

        var tz = CetTime.GetTimeZone();
        var thresholdUtc = justSent ? nowUtc.AddMinutes(1) : nowUtc;

        if (schedule.Count > 0)
        {
            return ComputeNextCustomUtc(schedule, tz, thresholdUtc);
        }

        return ComputeNextDefaultBiWeeklyUtc(config, tz, thresholdUtc);
    }

    private static DateTimeOffset ComputeNextCustomUtc(Dictionary<DayOfWeek, int> schedule, TimeZoneInfo tz, DateTimeOffset thresholdUtc)
    {
        var thresholdLocal = TimeZoneInfo.ConvertTimeFromUtc(thresholdUtc.UtcDateTime, tz);
        var startDate = thresholdLocal.Date;

        for (var i = 0; i < 14; i++)
        {
            var date = startDate.AddDays(i);
            if (!schedule.TryGetValue(date.DayOfWeek, out var hour))
            {
                continue;
            }

            var localCandidate = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0, DateTimeKind.Unspecified);
            var utcCandidate = LocalWallClockToUtc(localCandidate, tz);

            if (utcCandidate > thresholdUtc)
            {
                return utcCandidate;
            }
        }

        var fallbackDate = startDate.AddDays(7);
        for (var i = 0; i < 7; i++)
        {
            var date = fallbackDate.AddDays(i);
            if (!schedule.TryGetValue(date.DayOfWeek, out var hour))
            {
                continue;
            }

            var localCandidate = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0, DateTimeKind.Unspecified);
            return LocalWallClockToUtc(localCandidate, tz);
        }

        return thresholdUtc.AddDays(7);
    }

    private static DateTimeOffset ComputeNextDefaultBiWeeklyUtc(UserConfig config, TimeZoneInfo tz, DateTimeOffset thresholdUtc)
    {
        DateTime anchorLocalDate;

        if (config.StudyReminderNextAtUtc.HasValue)
        {
            var anchorLocal = TimeZoneInfo.ConvertTimeFromUtc(config.StudyReminderNextAtUtc.Value.UtcDateTime, tz);
            anchorLocalDate = anchorLocal.Date;
        }
        else
        {
            var thresholdLocal = TimeZoneInfo.ConvertTimeFromUtc(thresholdUtc.UtcDateTime, tz);
            anchorLocalDate = NextMondayDate(thresholdLocal.Date, thresholdLocal.DayOfWeek);
        }

        var candidateUtc = LocalWallClockToUtc(new DateTime(anchorLocalDate.Year, anchorLocalDate.Month, anchorLocalDate.Day, 8, 0, 0, DateTimeKind.Unspecified), tz);

        while (candidateUtc <= thresholdUtc)
        {
            anchorLocalDate = anchorLocalDate.AddDays(14);
            candidateUtc = LocalWallClockToUtc(new DateTime(anchorLocalDate.Year, anchorLocalDate.Month, anchorLocalDate.Day, 8, 0, 0, DateTimeKind.Unspecified), tz);
        }

        return candidateUtc;
    }

    private static DateTime NextMondayDate(DateTime startDate, DayOfWeek startDow)
    {
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)startDow + 7) % 7;
        var candidate = startDate.AddDays(daysUntilMonday);
        if (candidate == startDate)
        {
            return candidate.AddDays(7);
        }
        return candidate;
    }

    private static DateTimeOffset LocalWallClockToUtc(DateTime localUnspecified, TimeZoneInfo tz)
    {
        var adjusted = localUnspecified;

        for (var i = 0; i < 3 && tz.IsInvalidTime(adjusted); i++)
        {
            adjusted = adjusted.AddHours(1);
        }

        TimeSpan offset;

        if (tz.IsAmbiguousTime(adjusted))
        {
            var offsets = tz.GetAmbiguousTimeOffsets(adjusted);
            offset = offsets.Length > 0 ? offsets.Max() : tz.GetUtcOffset(adjusted);
        }
        else
        {
            offset = tz.GetUtcOffset(adjusted);
        }

        return new DateTimeOffset(adjusted, offset).ToUniversalTime();
    }
}
