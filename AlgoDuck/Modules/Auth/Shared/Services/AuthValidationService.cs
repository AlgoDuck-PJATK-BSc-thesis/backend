using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Modules.Auth.Shared.Services;

public sealed class AuthValidationService
{
    private readonly IAuthValidator _validator;

    public AuthValidationService(IAuthValidator validator)
    {
        _validator = validator;
    }

    public Task ValidateRegistrationAsync(string userName, string email, string password, CancellationToken cancellationToken)
    {
        return _validator.ValidateRegistrationAsync(userName, email, password, cancellationToken);
    }

    public Task ValidateLoginAsync(string userNameOrEmail, string password, CancellationToken cancellationToken)
    {
        return _validator.ValidateLoginAsync(userNameOrEmail, password, cancellationToken);
    }

    public Task ValidateEmailConfirmationAsync(Guid userId, string token, CancellationToken cancellationToken)
    {
        return _validator.ValidateEmailConfirmationAsync(userId, token, cancellationToken);
    }

    public Task ValidatePasswordChangeAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        return _validator.ValidatePasswordChangeAsync(userId, currentPassword, newPassword, cancellationToken);
    }
}