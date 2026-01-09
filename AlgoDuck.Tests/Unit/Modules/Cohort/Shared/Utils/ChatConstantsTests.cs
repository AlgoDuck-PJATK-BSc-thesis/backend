using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Shared.Utils;

public sealed class ChatConstantsTests
{
    [Fact]
    public void Values_AreConfiguredAsExpected()
    {
        Assert.Equal(512, ChatConstants.MaxMessageLength);
        Assert.Equal(50, ChatConstants.DefaultPageSize);
        Assert.Equal(100, ChatConstants.MaxPageSize);
    }
}