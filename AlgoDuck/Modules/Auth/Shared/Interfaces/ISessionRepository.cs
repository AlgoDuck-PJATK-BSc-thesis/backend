using AlgoDuck.Models;

namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface ISessionRepository
{
    Task AddAsync(Session session, CancellationToken cancellationToken);
    Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Session>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}