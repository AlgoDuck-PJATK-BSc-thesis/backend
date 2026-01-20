namespace AlgoDuck.Modules.User.Shared.Interfaces;

public interface IReminderEmailSender
{
    Task SendStudyReminderAsync(Guid userId, string email, CancellationToken cancellationToken);
}
