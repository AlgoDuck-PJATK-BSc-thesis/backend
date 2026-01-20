using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Utils;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Shared.Services;

public sealed class EmailSender : IEmailSender
{
    private readonly IEmailTransport _transport;
    private readonly UserManager<ApplicationUser> _userManager;

    public EmailSender(IEmailTransport transport, UserManager<ApplicationUser> userManager)
    {
        _transport = transport;
        _userManager = userManager;
    }

    private async Task<string> GetUserDisplayNameAsync(Guid userId, string fallback)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || string.IsNullOrWhiteSpace(user.UserName))
        {
            return fallback;
        }

        return user.UserName;
    }

    public async Task SendEmailConfirmationAsync(Guid userId, string email, string confirmationLink, CancellationToken cancellationToken)
    {
        var userName = await GetUserDisplayNameAsync(userId, email);
        var template = EmailTemplateRenderer.RenderEmailConfirmation(userName, confirmationLink);
        await _transport.SendAsync(email, template.Subject, template.Body, null, cancellationToken);
    }

    public async Task SendPasswordResetAsync(Guid userId, string email, string resetLink, CancellationToken cancellationToken)
    {
        var userName = await GetUserDisplayNameAsync(userId, email);
        var template = EmailTemplateRenderer.RenderPasswordReset(userName, resetLink);
        await _transport.SendAsync(email, template.Subject, template.Body, null, cancellationToken);
    }

    public async Task SendTwoFactorCodeAsync(Guid userId, string email, string code, CancellationToken cancellationToken)
    {
        var userName = await GetUserDisplayNameAsync(userId, email);
        var template = EmailTemplateRenderer.RenderTwoFactorCode(userName, code);
        await _transport.SendAsync(email, template.Subject, template.Body, null, cancellationToken);
    }

    public async Task SendEmailChangeConfirmationAsync(Guid userId, string newEmail, string confirmationLink, CancellationToken cancellationToken)
    {
        var userName = await GetUserDisplayNameAsync(userId, newEmail);
        var template = EmailTemplateRenderer.RenderEmailChangeConfirmation(userName, newEmail, confirmationLink);
        await _transport.SendAsync(newEmail, template.Subject, template.Body, null, cancellationToken);
    }
}