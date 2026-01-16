using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Integration.TestHost;

public sealed class FakeEmailTransport : IEmailTransport
{
    private readonly object _gate = new();
    private readonly List<CapturedEmail> _messages = new();

    public Task SendAsync(string to, string subject, string textBody, string? htmlBody = null, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _messages.Add(new CapturedEmail(to, subject, textBody, htmlBody, DateTimeOffset.UtcNow));
        }

        return Task.CompletedTask;
    }

    public void Clear()
    {
        lock (_gate)
        {
            _messages.Clear();
        }
    }

    public CapturedEmail? FindLastTo(string to)
    {
        lock (_gate)
        {
            for (var i = _messages.Count - 1; i >= 0; i--)
            {
                if (string.Equals(_messages[i].To, to, StringComparison.OrdinalIgnoreCase))
                {
                    return _messages[i];
                }
            }

            return null;
        }
    }
}