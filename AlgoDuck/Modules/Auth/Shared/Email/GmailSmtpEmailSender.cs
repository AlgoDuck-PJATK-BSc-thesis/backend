using System.Net;
using System.Net.Mail;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Modules.Auth.Shared.Email;

public sealed class GmailSmtpEmailSender : IEmailTransport
{
    private readonly string _smtpEmail;
    private readonly string _smtpPassword;
    private readonly string _from;

    public GmailSmtpEmailSender(IConfiguration configuration)
    {
        _smtpEmail =
            Environment.GetEnvironmentVariable("GMAIL__SMTP_EMAIL") ??
            configuration["Gmail:SmtpEmail"] ??
            throw new InvalidOperationException("Missing GMAIL__SMTP_EMAIL or Gmail:SmtpEmail");

        _smtpPassword =
            Environment.GetEnvironmentVariable("GMAIL__SMTP_PASSWORD") ??
            configuration["Gmail:SmtpPassword"] ??
            throw new InvalidOperationException("Missing GMAIL__SMTP_PASSWORD or Gmail:SmtpPassword");

        _from =
            Environment.GetEnvironmentVariable("EMAIL__FROM") ??
            configuration["Email:From"] ??
            _smtpEmail;
    }

    public async Task SendAsync(string to, string subject, string textBody, string? htmlBody = null, CancellationToken cancellationToken = default)
    {
        using var message = new MailMessage();
        message.From = new MailAddress(_from);
        message.To.Add(new MailAddress(to));
        message.Subject = subject;
        message.Body = htmlBody ?? textBody;
        message.IsBodyHtml = htmlBody is not null;

        using var client = new SmtpClient("smtp.gmail.com", 587);
        client.EnableSsl = true;
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(_smtpEmail, _smtpPassword);

        await client.SendMailAsync(message, cancellationToken);
    }
}