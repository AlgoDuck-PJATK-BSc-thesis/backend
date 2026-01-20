using AlgoDuck.Modules.Auth.Shared.Email;
using Microsoft.Extensions.Configuration;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Email;

public sealed class PostmarkEmailSenderTests
{
    [Fact]
    public void Ctor_WhenNoApiKeySources_Throws()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        Environment.SetEnvironmentVariable("EMAIL__POSTMARKAPIKEY", null);
        Environment.SetEnvironmentVariable("POSTMARK__SERVERAPIKEY", null);
        Environment.SetEnvironmentVariable("EMAIL__FROM", null);
        Environment.SetEnvironmentVariable("POSTMARK__MESSAGESTREAM", null);

        var ex = Assert.Throws<InvalidOperationException>(() => new PostmarkEmailSender(configuration));

        Assert.Contains("Missing EMAIL__POSTMARKAPIKEY", ex.Message);
    }

    [Fact]
    public void Ctor_WhenApiKeyProvidedButNoFrom_Throws()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Email:PostmarkApiKey"] = "key" })
            .Build();

        Environment.SetEnvironmentVariable("EMAIL__POSTMARKAPIKEY", null);
        Environment.SetEnvironmentVariable("POSTMARK__SERVERAPIKEY", null);
        Environment.SetEnvironmentVariable("EMAIL__FROM", null);

        var ex = Assert.Throws<InvalidOperationException>(() => new PostmarkEmailSender(configuration));

        Assert.Contains("Missing EMAIL__FROM", ex.Message);
    }

    [Fact]
    public void Ctor_WhenApiKeyAndFromProvided_UsesDefaultMessageStreamOutbound()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Email:PostmarkApiKey"] = "key", ["Email:From"] = "from@example.com" })
            .Build();

        Environment.SetEnvironmentVariable("EMAIL__POSTMARKAPIKEY", null);
        Environment.SetEnvironmentVariable("POSTMARK__SERVERAPIKEY", null);
        Environment.SetEnvironmentVariable("EMAIL__FROM", null);
        Environment.SetEnvironmentVariable("POSTMARK__MESSAGESTREAM", null);

        var sender = new PostmarkEmailSender(configuration);

        Assert.NotNull(sender);
    }
}
