using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Utils;

public sealed class ChatPresenceSettingsTests
{
    [Fact]
    public void Defaults_AreConfigured()
    {
        var settings = new ChatPresenceSettings();

        Assert.Equal(TimeSpan.FromMinutes(5), settings.IdleTimeout);
    }
}