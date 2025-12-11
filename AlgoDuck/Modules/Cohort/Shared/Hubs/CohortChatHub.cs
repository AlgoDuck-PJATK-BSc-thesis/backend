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
    private readonly ILogger<CohortChatHub> _logger;

    public CohortChatHub(
        ISendMessageHandler sendMessageHandler,
        ICohortRepository cohortRepository,
        IChatPresenceService chatPresenceService,
        ILogger<CohortChatHub> logger)
    {
        _sendMessageHandler = sendMessageHandler;
        _cohortRepository = cohortRepository;
        _chatPresenceService = chatPresenceService;
        _logger = logger;
    }

    public async Task SendMessage(SendMessageDto dto)
    {
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            await Clients.Caller.SendAsync("MessageRejected", "Unauthorized user.");
            return;
        }

        try
        {
            var result = await _sendMessageHandler.HandleAsync(userId, dto, CancellationToken.None);

            await Clients.Group(GetGroupName(dto.CohortId))
                .SendAsync("ReceiveMessage", result);
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

            await Clients.Caller.SendAsync("MessageRejected", reason);
        }
        catch (CohortValidationException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation error for user {UserId} in cohort {CohortId}",
                userId,
                dto.CohortId);

            await Clients.Caller.SendAsync("MessageRejected", "You cannot send messages to this cohort.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while sending message for user {UserId} in cohort {CohortId}",
                userId,
                dto.CohortId);

            await Clients.Caller.SendAsync("MessageRejected", "Internal error. Please try again.");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var http = Context.GetHttpContext();
        var cohortIdRaw = http?.Request.Query["cohortId"].ToString();
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(cohortIdRaw, out var cohortId) || string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
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
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static string GetGroupName(Guid cohortId)
    {
        return $"cohort:{cohortId}";
    }
}