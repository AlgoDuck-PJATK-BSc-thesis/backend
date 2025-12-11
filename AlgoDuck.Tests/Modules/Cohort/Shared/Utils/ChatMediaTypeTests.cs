using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Utils;

public sealed class ChatMediaTypeTests
{
    [Fact]
    public void EnumValues_AreStable()
    {
        Assert.Equal(0, (int)ChatMediaType.Text);
        Assert.Equal(1, (int)ChatMediaType.Image);
    }
}