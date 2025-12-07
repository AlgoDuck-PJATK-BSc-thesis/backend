namespace AlgoDuck.Modules.Auth.Commands.ChangeEmailConfirm;

public interface IChangeEmailConfirmHandler
{
    Task HandleAsync(ChangeEmailConfirmDto dto, CancellationToken cancellationToken);
}