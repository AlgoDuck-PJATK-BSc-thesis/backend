using PostmarkDotNet;

namespace AlgoDuck.Modules.Auth.Email
{
    public sealed class PostmarkEmailSender : IEmailSender
    {
        private readonly string _apiKey;
        private readonly string _from;

        public PostmarkEmailSender(IConfiguration cfg)
        {
            _apiKey = Environment.GetEnvironmentVariable("POSTMARK__SERVERAPIKEY")
                      ?? cfg["Email:PostmarkApiKey"]
                      ?? throw new InvalidOperationException("Missing POSTMARK__SERVERAPIKEY or Email:PostmarkApiKey");
            _from = Environment.GetEnvironmentVariable("EMAIL__FROM")
                    ?? cfg["Email:From"]
                    ?? throw new InvalidOperationException("Missing EMAIL__FROM or Email:From");
        }

        public async Task SendAsync(string to, string subject, string textBody, string? htmlBody = null, CancellationToken ct = default)
        {
            var client = new PostmarkClient(_apiKey);
            var msg = new PostmarkMessage
            {
                To = to,
                From = _from,
                Subject = subject,
                TextBody = textBody,
                HtmlBody = htmlBody ?? $"<pre>{System.Net.WebUtility.HtmlEncode(textBody)}</pre>",
                MessageStream = "outbound"
            };
            var res = await client.SendMessageAsync(msg);
            if (res.Status != PostmarkStatus.Success)
                throw new InvalidOperationException($"Postmark send failed: {res.Message}");
        }
    }
}