using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Modules.Auth.Shared.Services;

public sealed class ExternalAuthService
{
    private readonly IExternalAuthProvider _externalAuthProvider;
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;
    private readonly IAuthValidator _authValidator;

    public ExternalAuthService(
        IExternalAuthProvider externalAuthProvider,
        IAuthRepository authRepository,
        ITokenService tokenService,
        IAuthValidator authValidator)
    {
        _externalAuthProvider = externalAuthProvider;
        _authRepository = authRepository;
        _tokenService = tokenService;
        _authValidator = authValidator;
    }

    public async Task<AuthResponse> LoginWithProviderAsync(string provider, string accessToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new ValidationException("Provider is required.");
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ValidationException("Access token is required.");
        }

        var externalUser = await _externalAuthProvider.GetUserInfoAsync(provider, accessToken, cancellationToken);
        if (externalUser is null)
        {
            throw new ValidationException("Could not retrieve user information from external provider.");
        }

        if (string.IsNullOrWhiteSpace(externalUser.Email))
        {
            throw new ValidationException("External provider did not return an email address.");
        }

        var user = await _authRepository.FindByEmailAsync(externalUser.Email, cancellationToken);
        if (user is null)
        {
            throw new ValidationException("No account exists for this email. Please register first.");
        }

        await _authValidator.ValidateLoginAsync(externalUser.Email, string.Empty, cancellationToken);

        return await _tokenService.GenerateAuthTokensAsync(user, cancellationToken);
    }
}