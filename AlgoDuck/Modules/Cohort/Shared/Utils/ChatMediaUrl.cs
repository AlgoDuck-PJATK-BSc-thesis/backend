namespace AlgoDuck.Modules.Cohort.Shared.Utils;

public static class ChatMediaUrl
{
    public static string Build(Guid cohortId, string key)
    {
        var encoded = Uri.EscapeDataString(key);
        return $"/api/cohorts/{cohortId}/chat/media?key={encoded}";
    }

    public static bool KeyBelongsToCohort(ChatMediaSettings settings, Guid cohortId, string key)
    {
        var prefix = $"{settings.RootPrefix}/cohorts/{cohortId}/";
        return key.StartsWith(prefix, StringComparison.Ordinal);
    }
}