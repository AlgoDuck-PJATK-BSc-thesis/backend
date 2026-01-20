namespace AlgoDuck.Tests.Integration.TestHost;

public sealed record CapturedEmail(
    string To,
    string Subject,
    string TextBody,
    string? HtmlBody,
    DateTimeOffset SentAtUtc);