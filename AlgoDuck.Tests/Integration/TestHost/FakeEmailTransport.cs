using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Integration.TestHost;

public sealed class FakeEmailTransport : IEmailTransport
{
    public Task SendAsync(string to, string subject, string textBody, string? htmlBody = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}