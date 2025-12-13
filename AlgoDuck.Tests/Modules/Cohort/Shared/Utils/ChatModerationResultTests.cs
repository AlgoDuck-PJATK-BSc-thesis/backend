using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Utils;

public sealed class ChatModerationResultTests
{
    [Fact]
    public void Allowed_Factory_SetsIsAllowedTrue()
    {
        var result = ChatModerationResult.Allowed();

        Assert.True(result.IsAllowed);
        Assert.Null(result.BlockReason);
        Assert.Null(result.Category);
        Assert.Null(result.Severity);
    }

    [Fact]
    public void Blocked_Factory_SetsFields()
    {
        var result = ChatModerationResult.Blocked("reason", "category", 0.9);

        Assert.False(result.IsAllowed);
        Assert.Equal("reason", result.BlockReason);
        Assert.Equal("category", result.Category);
        Assert.Equal(0.9, result.Severity);
    }
}