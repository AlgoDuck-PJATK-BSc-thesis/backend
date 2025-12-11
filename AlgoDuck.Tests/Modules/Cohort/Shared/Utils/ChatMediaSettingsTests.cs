using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Utils;

public sealed class ChatMediaSettingsTests
{
    [Fact]
    public void Defaults_AreConfigured()
    {
        var settings = new ChatMediaSettings();

        Assert.Equal(64L * 1024L * 1024L, settings.MaxFileSizeBytes);
        Assert.Equal("cohort-chat", settings.RootPrefix);
        Assert.NotNull(settings.AllowedContentTypes);
        Assert.Equal(2, settings.AllowedContentTypes.Length);
        Assert.Contains("image/jpeg", settings.AllowedContentTypes);
        Assert.Contains("image/png", settings.AllowedContentTypes);
    }
}