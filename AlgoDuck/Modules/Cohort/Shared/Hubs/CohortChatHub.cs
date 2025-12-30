using System.Security.Claims;
using AlgoDuck.Modules.Cohort.Commands.Chat.SendMessage;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AlgoDuck.Modules.Cohort.Shared.Hubs;

[Authorize]
public class CohortChatHub : Hub
{
    private readonly ISendMessageHandler _sendMessageHandler;
    private readonly ICohortRepository _cohortRepository;
    private readonly IChatPresenceService _chatPresenceService;
    private readonly IChatReadReceiptService _chatReadReceiptService;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly ILogger<CohortChatHub> _logger;

    public CohortChatHub(
        ISendMessageHandler sendMessageHandler,
        ICohortRepository cohortRepository,
        IChatPresenceService chatPresenceService,
        IChatReadReceiptService chatReadReceiptService,
        IChatMessageRepository chatMessageRepository,
        ILogger<CohortChatHub> logger)
    {
        _sendMessageHandler = sendMessageHandler;
        _cohortRepository = cohortRepository;
        _chatPresenceService = chatPresenceService;
        _chatReadReceiptService = chatReadReceiptService;
        _chatMessageRepository = chatMessageRepository;
        _logger = logger;
    }

    public async Task SendMessage(SendMessageDto dto)
    {
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            await Clients.Caller.SendAsync("MessageRejected", "Unauthorized user.", Context.ConnectionAborted);
            return;
        }

        try
        {
            var result = await _sendMessageHandler.HandleAsync(userId, dto, Context.ConnectionAborted);

            await Clients.Group(GetGroupName(dto.CohortId))
                .SendAsync("ReceiveMessage", result, Context.ConnectionAborted);
        }
        catch (ChatValidationException ex)
        {
            _logger.LogInformation(
                ex,
                "Chat message rejected for user {UserId} in cohort {CohortId}",
                userId,
                dto.CohortId);

            var reason = string.IsNullOrWhiteSpace(ex.Message)
                ? "This message violates our content rules."
                : ex.Message;

            await Clients.Caller.SendAsync("MessageRejected", reason, Context.ConnectionAborted);
        }
        catch (CohortValidationException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error for user {UserId} in cohort {CohortId}",
                userId,
                dto.CohortId);

            await Clients.Caller.SendAsync(
                "MessageRejected",
                "You cannot send messages to this cohort.",
                Context.ConnectionAborted);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while sending message for user {UserId} in cohort {CohortId}",
                userId,
                dto.CohortId);

            await Clients.Caller.SendAsync(
                "MessageRejected",
                "Internal error. Please try again.",
                Context.ConnectionAborted);
        }
    }

    public async Task MarkReadUpTo(Guid messageId)
    {
        var http = Context.GetHttpContext();
        var cohortIdRaw = http?.Request.Query["cohortId"].ToString();
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(cohortIdRaw, out var cohortId) ||
            string.IsNullOrWhiteSpace(userIdStr) ||
            !Guid.TryParse(userIdStr, out var userId))
        {
            return;
        }

        var belongs = await _cohortRepository.UserBelongsToCohortAsync(userId, cohortId, Context.ConnectionAborted);
        if (!belongs)
        {
            return;
        }

        var message = await _chatMessageRepository.GetByIdForCohortAsync(cohortId, messageId, Context.ConnectionAborted);
        if (message is null)
        {
            return;
        }

        var readByCount = await _chatReadReceiptService.MarkReadUpToAsync(cohortId, userId, message, Context.ConnectionAborted);

        await Clients.Group(GetGroupName(cohortId)).SendAsync(
            "ReadReceiptUpdated",
            new
            {
                messageId = message.MessageId,
                readByCount
            },
            Context.ConnectionAborted
        );
    }

    public async Task ReportActivity()
    {
        var http = Context.GetHttpContext();
        var cohortIdRaw = http?.Request.Query["cohortId"].ToString();
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(cohortIdRaw, out var cohortId) ||
            string.IsNullOrWhiteSpace(userIdStr) ||
            !Guid.TryParse(userIdStr, out var userId))
        {
            return;
        }

        var snapshot = await _chatPresenceService.ReportActivityAsync(cohortId, userId, Context.ConnectionId, Context.ConnectionAborted);

        await Clients.Group(GetGroupName(cohortId)).SendAsync(
            "PresenceUpdated",
            new
            {
                userId,
                isActive = snapshot.IsActiveLegacy,
                status = snapshot.Status.ToString(),
                lastActivityAt = snapshot.LastActivityAt,
                lastSeenAt = snapshot.LastSeenAt,
                connectionCount = snapshot.ConnectionCount
            },
            Context.ConnectionAborted
        );
    }

    public async Task RequestPresenceSnapshot()
    {
        var http = Context.GetHttpContext();
        var cohortIdRaw = http?.Request.Query["cohortId"].ToString();

        if (!Guid.TryParse(cohortIdRaw, out var cohortId))
        {
            return;
        }

        var list = await _chatPresenceService.GetSnapshotsForCohortAsync(cohortId, Context.ConnectionAborted);

        await Clients.Caller.SendAsync(
            "PresenceSnapshot",
            list.Select(x => new
            {
                userId = x.UserId,
                isActive = x.IsActiveLegacy,
                status = x.Status.ToString(),
                lastActivityAt = x.LastActivityAt,
                lastSeenAt = x.LastSeenAt,
                connectionCount = x.ConnectionCount
            }),
            Context.ConnectionAborted
        );
    }

    public override async Task OnConnectedAsync()
    {
        var http = Context.GetHttpContext();
        var cohortIdRaw = http?.Request.Query["cohortId"].ToString();
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(cohortIdRaw, out var cohortId) ||
            string.IsNullOrWhiteSpace(userIdStr) ||
            !Guid.TryParse(userIdStr, out var userId))
        {
            _logger.LogWarning("Missing or invalid cohortId/userId; aborting connection");
            Context.Abort();
            return;
        }

        var belongs = await _cohortRepository.UserBelongsToCohortAsync(userId, cohortId, Context.ConnectionAborted);
        if (!belongs)
        {
            _logger.LogWarning("User {UserId} denied joining cohort {CohortId}", userId, cohortId);
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(cohortId), Context.ConnectionAborted);
        await _chatPresenceService.UserConnectedAsync(cohortId, userId, Context.ConnectionId, Context.ConnectionAborted);

        var snapshot = await _chatPresenceService.GetSnapshotAsync(cohortId, userId, Context.ConnectionAborted);

        await Clients.Group(GetGroupName(cohortId)).SendAsync(
            "PresenceUpdated",
            new
            {
                userId,
                isActive = snapshot.IsActiveLegacy,
                status = snapshot.Status.ToString(),
                lastActivityAt = snapshot.LastActivityAt,
                lastSeenAt = snapshot.LastSeenAt,
                connectionCount = snapshot.ConnectionCount
            },
            Context.ConnectionAborted
        );

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var http = Context.GetHttpContext();
        var cohortIdRaw = http?.Request.Query["cohortId"].ToString();
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(cohortIdRaw, out var cohortId) && Guid.TryParse(userIdStr, out var userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(cohortId), Context.ConnectionAborted);
            await _chatPresenceService.UserDisconnectedAsync(cohortId, userId, Context.ConnectionId, Context.ConnectionAborted);

            var snapshot = await _chatPresenceService.GetSnapshotAsync(cohortId, userId, Context.ConnectionAborted);

            await Clients.Group(GetGroupName(cohortId)).SendAsync(
                "PresenceUpdated",
                new
                {
                    userId,
                    isActive = snapshot.IsActiveLegacy,
                    status = snapshot.Status.ToString(),
                    lastActivityAt = snapshot.LastActivityAt,
                    lastSeenAt = snapshot.LastSeenAt,
                    connectionCount = snapshot.ConnectionCount
                },
                Context.ConnectionAborted
            );
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static string GetGroupName(Guid cohortId)
    {
        return $"cohort:{cohortId}";
    }
}
