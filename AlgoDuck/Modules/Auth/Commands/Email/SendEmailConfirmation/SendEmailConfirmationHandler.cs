using System.Text;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace AlgoDuck.Modules.Auth.Commands.Email.SendEmailConfirmation;

public sealed class SendEmailConfirmationCommand : IRequest
{
    public Guid UserId { get; }

    public SendEmailConfirmationCommand(Guid userId)
    {
        UserId = userId;
    }
}

public sealed class SendEmailConfirmationHandler : IRequestHandler<SendEmailConfirmationCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

#if DEBUG
    private const string BaseUrl = "http://localhost:8080";
#else
    private const string BaseUrl = "https://algoduck.pl";
#endif

    public SendEmailConfirmationHandler(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    public async Task Handle(SendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedTokenBytes = Encoding.UTF8.GetBytes(token);
        var encodedToken = WebEncoders.Base64UrlEncode(encodedTokenBytes);

        var confirmationUrl = $"{BaseUrl}/auth/email-verification?userId={user.Id}&token={encodedToken}";

        await _emailSender.SendEmailConfirmationAsync(user.Id, user.Email, confirmationUrl, cancellationToken);
    }
}