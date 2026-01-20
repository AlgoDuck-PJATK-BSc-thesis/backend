using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Utils;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.User.Shared.Services;

public sealed class ReminderEmailSender : IReminderEmailSender
{
    private readonly IEmailTransport _transport;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReminderEmailSender(IEmailTransport transport, UserManager<ApplicationUser> userManager)
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

    public async Task SendStudyReminderAsync(Guid userId, string email, CancellationToken cancellationToken)
    {
        var userName = await GetUserDisplayNameAsync(userId, email);
        var template = EmailTemplateRenderer.RenderStudyReminder(userName);
        await _transport.SendAsync(email, template.Subject, template.Body, null, cancellationToken);
    }
}