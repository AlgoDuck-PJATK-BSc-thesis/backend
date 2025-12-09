using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Cohort.Shared.Repositories;

public sealed class ChatMessageRepository : IChatMessageRepository
{
    private readonly ApplicationQueryDbContext _queryDb;
    private readonly ApplicationCommandDbContext _commandDb;

    public ChatMessageRepository(ApplicationQueryDbContext queryDb, ApplicationCommandDbContext commandDb)
    {
        _queryDb = queryDb;
        _commandDb = commandDb;
    }

    public async Task<Message> AddAsync(Message message, CancellationToken cancellationToken)
    {
        _commandDb.Messages.Add(message);
        await _commandDb.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task<IReadOnlyList<Message>> GetPagedForCohortAsync(
        Guid cohortId,
        DateTime? beforeCreatedAtUtc,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (pageSize <= 0)
        {
            pageSize = ChatConstants.DefaultPageSize;
        }

        if (pageSize > ChatConstants.MaxPageSize)
        {
            pageSize = ChatConstants.MaxPageSize;
        }

        var query = _queryDb.Messages
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => m.CohortId == cohortId);

        if (beforeCreatedAtUtc.HasValue)
        {
            query = query.Where(m => m.CreatedAt < beforeCreatedAtUtc.Value);
        }

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SoftDeleteAsync(Guid messageId, Guid requesterId, CancellationToken cancellationToken)
    {
        var message = await _commandDb.Messages
            .FirstOrDefaultAsync(m => m.MessageId == messageId && m.UserId == requesterId, cancellationToken);

        if (message is null)
        {
            return false;
        }

        _commandDb.Messages.Remove(message);
        await _commandDb.SaveChangesAsync(cancellationToken);
        return true;
    }
}