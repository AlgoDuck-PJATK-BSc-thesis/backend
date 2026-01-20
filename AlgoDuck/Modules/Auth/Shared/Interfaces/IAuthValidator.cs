namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface IAuthValidator
{
    Task ValidateRegistrationAsync(string userName, string email, string password, CancellationToken cancellationToken);
    Task ValidateLoginAsync(string userNameOrEmail, string password, CancellationToken cancellationToken);
    Task ValidateEmailConfirmationAsync(Guid userId, string token, CancellationToken cancellationToken);
    Task ValidatePasswordChangeAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
}