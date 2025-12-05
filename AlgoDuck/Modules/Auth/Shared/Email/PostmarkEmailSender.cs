using AlgoDuck.Modules.Auth.Shared.Interfaces;
using PostmarkDotNet;

namespace AlgoDuck.Modules.Auth.Shared.Email;

public sealed class PostmarkEmailSender : IEmailTransport
{
    private readonly string _apiKey;
    private readonly string _from;

    public PostmarkEmailSender(IConfiguration configuration)
    {
        _apiKey = Environment.GetEnvironmentVariable("POSTMARK__SERVERAPIKEY")
                  ?? configuration["Email:PostmarkApiKey"]
                  ?? throw new InvalidOperationException("Missing POSTMARK__SERVERAPIKEY or Email:PostmarkApiKey");

        _from = Environment.GetEnvironmentVariable("EMAIL__FROM")
                ?? configuration["Email:From"]
                ?? throw new InvalidOperationException("Missing EMAIL__FROM or Email:From");
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
            MessageStream = "outbound"
        };

        var response = await client.SendMessageAsync(message);
        if (response.Status != PostmarkStatus.Success)
        {
            throw new InvalidOperationException($"Postmark send failed: {response.Message}");
        }
    }
}