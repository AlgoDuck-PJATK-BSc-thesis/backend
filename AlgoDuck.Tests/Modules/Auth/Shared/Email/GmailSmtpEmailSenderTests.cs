using AlgoDuck.Modules.Auth.Shared.Email;
using Microsoft.Extensions.Configuration;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Email;

public sealed class GmailSmtpEmailSenderTests
{
    [Fact]
    public void Ctor_WhenMissingEnvVars_Throws()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        Environment.SetEnvironmentVariable("GMAIL__SMTP_EMAIL", null);
        Environment.SetEnvironmentVariable("GMAIL__SMTP_PASSWORD", null);
        Environment.SetEnvironmentVariable("EMAIL__FROM", null);

        var ex = Assert.Throws<InvalidOperationException>(() => new GmailSmtpEmailSender(configuration));

        Assert.Contains("GMAIL__SMTP_EMAIL", ex.Message);
    }

    [Fact]
    public void Ctor_WhenEnvVarsPresent_Creates()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        Environment.SetEnvironmentVariable("GMAIL__SMTP_EMAIL", "test@example.com");
        Environment.SetEnvironmentVariable("GMAIL__SMTP_PASSWORD", "pw");
        Environment.SetEnvironmentVariable("EMAIL__FROM", null);

        var sender = new GmailSmtpEmailSender(configuration);

        Assert.NotNull(sender);
    }
}