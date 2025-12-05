using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Auth.Shared.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly ApplicationCommandDbContext _commandDbContext;

    public SessionRepository(ApplicationCommandDbContext commandDbContext)
    {
        _commandDbContext = commandDbContext;
    }

    public async Task AddAsync(Session session, CancellationToken cancellationToken)
    {
        await _commandDbContext.Sessions.AddAsync(session, cancellationToken);
    }

    public async Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return await _commandDbContext.Sessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
    }

    public async Task<IReadOnlyList<Session>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _commandDbContext.Sessions
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _commandDbContext.SaveChangesAsync(cancellationToken);
    }
}