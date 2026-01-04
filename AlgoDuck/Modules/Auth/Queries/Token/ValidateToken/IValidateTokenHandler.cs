namespace AlgoDuck.Modules.Auth.Queries.Token.ValidateToken;

public interface IValidateTokenHandler
{
    Task<ValidateTokenResult> HandleAsync(ValidateTokenDto dto, CancellationToken cancellationToken);
}