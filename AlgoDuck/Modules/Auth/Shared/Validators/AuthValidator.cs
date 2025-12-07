using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Validators;
using AlgoDuck.Modules.Auth.Shared.Utils;

namespace AlgoDuck.Modules.Auth.Shared.Validators;

public sealed class AuthValidator : IAuthValidator
{
    private readonly IAuthRepository _authRepository;
    private readonly EmailValidator _emailValidator;
    private readonly PasswordValidator _passwordValidator;

    public AuthValidator(
        IAuthRepository authRepository,
        EmailValidator emailValidator,
        PasswordValidator passwordValidator)
    {
        _authRepository = authRepository;
        _emailValidator = emailValidator;
        _passwordValidator = passwordValidator;
    }

    public async Task ValidateRegistrationAsync(string userName, string email, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ValidationException("Username is required.");
        }

        if (userName.Length > ValidationRules.UserNameMaxLength)
        {
            throw new ValidationException($"Username must be at most {ValidationRules.UserNameMaxLength} characters long.");
        }

        _emailValidator.Validate(email);
        _passwordValidator.Validate(password);

        var existingByName = await _authRepository.FindByUserNameAsync(userName, cancellationToken);
        if (existingByName is not null)
        {
            throw new ValidationException("Username is already taken.");
        }

        var existingByEmail = await _authRepository.FindByEmailAsync(email, cancellationToken);
        if (existingByEmail is not null)
        {
            throw new ValidationException("Email is already in use.");
        }
    }

    public async Task ValidateLoginAsync(string userNameOrEmail, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail))
        {
            throw new ValidationException("Username or email is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ValidationException("Password is required.");
        }

        var byUserName = await _authRepository.FindByUserNameAsync(userNameOrEmail, cancellationToken);
        if (byUserName is not null)
        {
            return;
        }

        var byEmail = await _authRepository.FindByEmailAsync(userNameOrEmail, cancellationToken);
        if (byEmail is not null)
        {
            return;
        }

        throw new ValidationException("Invalid username or email.");
    }

    public Task ValidateEmailConfirmationAsync(Guid userId, string token, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new EmailVerificationException("User identifier is invalid.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new EmailVerificationException("Email confirmation token is required.");
        }

        return Task.CompletedTask;
    }

    public Task ValidatePasswordChangeAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ValidationException("User identifier is invalid.");
        }

        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            throw new ValidationException("Current password is required.");
        }

        _passwordValidator.Validate(newPassword);

        return Task.CompletedTask;
    }
}