namespace AlgoDuck.Modules.Auth.Queries.ValidateToken;

public interface IValidateTokenHandler
{
    Task<ValidateTokenResult> HandleAsync(ValidateTokenDto dto, CancellationToken cancellationToken);
}