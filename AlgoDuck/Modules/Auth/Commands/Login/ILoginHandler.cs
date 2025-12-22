namespace AlgoDuck.Modules.Auth.Commands.Login;

public interface ILoginHandler
{
    Task<LoginResult> HandleAsync(LoginDto dto, CancellationToken cancellationToken);
}