using System.Collections.Concurrent;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Tests.Integration.TestHost;

public sealed record CapturedEmail(
    string To,
    string Subject,
    string TextBody,
    string? HtmlBody,
    DateTimeOffset SentAtUtc);

public sealed class FakeEmailTransport : IEmailTransport
{
    private readonly ConcurrentQueue<CapturedEmail> _outbox = new();

    public Task SendAsync(string to, string subject, string textBody, string? htmlBody = null, CancellationToken cancellationToken = default)
    {
        _outbox.Enqueue(new CapturedEmail(
            to,
            subject,
            textBody,
            htmlBody,
            DateTimeOffset.UtcNow));

        return Task.CompletedTask;
    }

    public IReadOnlyList<CapturedEmail> GetAll()
    {
        return _outbox.ToArray();
    }

    public void Clear()
    {
        while (_outbox.TryDequeue(out _))
        {
        }
    }

    public CapturedEmail? FindLastTo(string to)
    {
        var all = _outbox.ToArray();
        for (var i = all.Length - 1; i >= 0; i--)
        {
            if (string.Equals(all[i].To, to, StringComparison.OrdinalIgnoreCase))
            {
                return all[i];
            }
        }

        return null;
    }
}