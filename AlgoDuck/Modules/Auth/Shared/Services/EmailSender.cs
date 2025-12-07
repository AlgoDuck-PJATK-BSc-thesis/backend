using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Utils;

namespace AlgoDuck.Modules.Auth.Shared.Services;

public sealed class EmailSender : IEmailSender
{
    private readonly IEmailTransport _transport;

    public EmailSender(IEmailTransport transport)
    {
        _transport = transport;
    }

    public async Task SendEmailConfirmationAsync(Guid userId, string email, string confirmationLink, CancellationToken cancellationToken)
    {
        var template = EmailTemplateRenderer.RenderEmailConfirmation(userId.ToString(), confirmationLink);
        await _transport.SendAsync(email, template.Subject, template.Body, null, cancellationToken);
    }

    public async Task SendPasswordResetAsync(Guid userId, string email, string resetLink, CancellationToken cancellationToken)
    {
        var template = EmailTemplateRenderer.RenderPasswordReset(userId.ToString(), resetLink);
        await _transport.SendAsync(email, template.Subject, template.Body, null, cancellationToken);
    }

    public async Task SendTwoFactorCodeAsync(Guid userId, string email, string code, CancellationToken cancellationToken)
    {
        var template = EmailTemplateRenderer.RenderTwoFactorCode(userId.ToString(), code);
        await _transport.SendAsync(email, template.Subject, template.Body, null, cancellationToken);
    }

    public async Task SendEmailChangeConfirmationAsync(Guid userId, string newEmail, string confirmationLink, CancellationToken cancellationToken)
    {
        var template = EmailTemplateRenderer.RenderEmailChangeConfirmation(userId.ToString(), newEmail, confirmationLink);
        await _transport.SendAsync(newEmail, template.Subject, template.Body, null, cancellationToken);
    }
}