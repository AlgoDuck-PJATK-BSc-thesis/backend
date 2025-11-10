namespace AlgoDuck.Modules.Auth.Email
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string textBody, string? htmlBody = null, CancellationToken ct = default);
    }
}