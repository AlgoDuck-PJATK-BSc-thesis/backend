namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface IEmailTransport
{
    Task SendAsync(string to, string subject, string textBody, string? htmlBody = null, CancellationToken cancellationToken = default);
}