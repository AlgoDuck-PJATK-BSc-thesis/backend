namespace AlgoDuck.Modules.User.Commands.DeleteAccount;

public interface IDeleteAccountHandler
{
    Task HandleAsync(Guid userId, DeleteAccountDto dto, CancellationToken cancellationToken);
}