namespace AlgoDuck.Shared.Utilities;

public static class CetTime
{
    public static TimeZoneInfo GetTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");
        }
        catch
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        }
    }

    public static DateTimeOffset UtcNow()
    {
        return DateTimeOffset.UtcNow;
    }

    public static DateTimeOffset NowLocal()
    {
        var tz = GetTimeZone();
        return TimeZoneInfo.ConvertTime(UtcNow(), tz);
    }

    public static DateTimeOffset ToLocal(DateTimeOffset utc)
    {
        var tz = GetTimeZone();
        return TimeZoneInfo.ConvertTime(utc, tz);
    }
}