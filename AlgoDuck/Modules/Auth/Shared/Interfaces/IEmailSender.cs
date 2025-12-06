namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(Guid userId, string email, string confirmationLink, CancellationToken cancellationToken);
    Task SendPasswordResetAsync(Guid userId, string email, string resetLink, CancellationToken cancellationToken);
    Task SendTwoFactorCodeAsync(Guid userId, string email, string code, CancellationToken cancellationToken);
    Task SendEmailChangeConfirmationAsync(Guid userId, string newEmail, string confirmationLink, CancellationToken cancellationToken);
}