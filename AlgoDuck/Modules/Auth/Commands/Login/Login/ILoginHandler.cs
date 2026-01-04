namespace AlgoDuck.Modules.Auth.Commands.Login.Login;

public interface ILoginHandler
{
    Task<LoginResult> HandleAsync(LoginDto dto, CancellationToken cancellationToken);
}