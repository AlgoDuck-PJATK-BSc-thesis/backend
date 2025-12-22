using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Utils;

public sealed class ChatMediaDescriptorTests
{
    [Fact]
    public void DefaultValues_AreInitialized()
    {
        var descriptor = new ChatMediaDescriptor();

        Assert.Equal(string.Empty, descriptor.Key);
        Assert.Equal(string.Empty, descriptor.Url);
        Assert.Equal(string.Empty, descriptor.ContentType);
        Assert.Equal(0, descriptor.SizeBytes);
        Assert.Equal(ChatMediaType.Text, descriptor.MediaType);
    }

    [Fact]
    public void ObjectInitializer_SetsProperties()
    {
        var key = "cohort-chat/cohorts/c/users/u/file.png";
        var url = "https://example.com/file.png";
        var contentType = "image/png";
        var size = 12345L;

        var descriptor = new ChatMediaDescriptor
        {
            Key = key,
            Url = url,
            ContentType = contentType,
            SizeBytes = size,
            MediaType = ChatMediaType.Image
        };

        Assert.Equal(key, descriptor.Key);
        Assert.Equal(url, descriptor.Url);
        Assert.Equal(contentType, descriptor.ContentType);
        Assert.Equal(size, descriptor.SizeBytes);
        Assert.Equal(ChatMediaType.Image, descriptor.MediaType);
    }
}