namespace AlgoDuck.Modules.User.Commands.User.Account.DeleteAccount;

public interface IDeleteAccountHandler
{
    Task HandleAsync(Guid userId, DeleteAccountDto dto, CancellationToken cancellationToken);
}