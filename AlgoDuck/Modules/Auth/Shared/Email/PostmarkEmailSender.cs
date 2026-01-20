using AlgoDuck.Modules.Auth.Shared.Interfaces;
using PostmarkDotNet;

namespace AlgoDuck.Modules.Auth.Shared.Email;

public sealed class PostmarkEmailSender : IEmailTransport
{
    private readonly string _apiKey;
    private readonly string _from;
    private readonly string _messageStream;

    public PostmarkEmailSender(IConfiguration configuration)
    {
        _apiKey =
            Environment.GetEnvironmentVariable("EMAIL__POSTMARKAPIKEY") ??
            Environment.GetEnvironmentVariable("POSTMARK__SERVERAPIKEY") ??
            configuration["Email:PostmarkApiKey"] ??
            throw new InvalidOperationException("Missing EMAIL__POSTMARKAPIKEY, POSTMARK__SERVERAPIKEY or Email:PostmarkApiKey");

        _from =
            Environment.GetEnvironmentVariable("EMAIL__FROM") ??
            configuration["Email:From"] ??
            throw new InvalidOperationException("Missing EMAIL__FROM or Email:From");

        _messageStream =
            Environment.GetEnvironmentVariable("POSTMARK__MESSAGESTREAM") ??
            configuration["Postmark:MessageStream"] ??
            "outbound";
    }

    public async Task SendAsync(string to, string subject, string textBody, string? htmlBody = null, CancellationToken cancellationToken = default)
    {
        var client = new PostmarkClient(_apiKey);
        var message = new PostmarkMessage
        {
            To = to,
            From = _from,
            Subject = subject,
            TextBody = textBody,
            HtmlBody = htmlBody ?? $"<pre>{System.Net.WebUtility.HtmlEncode(textBody)}</pre>",
            MessageStream = _messageStream
        };

        var response = await client.SendMessageAsync(message);
        if (response.Status != PostmarkStatus.Success)
        {
            throw new InvalidOperationException($"Postmark send failed: {response.Message}");
        }
    }
}