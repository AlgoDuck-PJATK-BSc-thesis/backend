namespace AlgoDuck.Modules.Auth.Commands.Email.ChangeEmailConfirm;

public interface IChangeEmailConfirmHandler
{
    Task HandleAsync(ChangeEmailConfirmDto dto, CancellationToken cancellationToken);
}