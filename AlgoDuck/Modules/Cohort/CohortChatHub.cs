using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using AlgoDuck.Modules.Cohort.DTOs;
using AlgoDuck.Modules.Cohort.Interfaces;

namespace AlgoDuck.Modules.Cohort
{
    [Authorize]
    public class CohortChatHub : Hub
    {
        private readonly ICohortChatService _chatService;
        private readonly ICohortService _cohortService;
        private readonly ILogger<CohortChatHub> _logger;

        public CohortChatHub(ICohortChatService chatService, ICohortService cohortService, ILogger<CohortChatHub> logger)
        {
            _chatService = chatService;
            _cohortService = cohortService;
            _logger = logger;
        }

        public async Task SendMessage(CohortChatDto dto)
        {
            var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr)) { Context.Abort(); return; }

            var userId = Guid.Parse(userIdStr);
            var cohortId = dto.CohortId;

            var belongs = await _cohortService.UserBelongsToCohortAsync(userId, cohortId, Context.ConnectionAborted);
            if (!belongs) { _logger.LogWarning("User {UserId} tried to send to cohort {CohortId} without membership", userId, cohortId); return; }

            var broadcast = await _chatService.SaveMessageAsync(dto);
            await Clients.Group($"cohort:{cohortId}").SendAsync("ReceiveMessage", broadcast, Context.ConnectionAborted);
        }

        public override async Task OnConnectedAsync()
        {
            var http = Context.GetHttpContext();
            var cohortIdRaw = http?.Request.Query["cohortId"].ToString();
            var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(cohortIdRaw, out var cohortId) || string.IsNullOrWhiteSpace(userIdStr))
            {
                _logger.LogWarning("Missing cohortId or userId; aborting connection");
                Context.Abort();
                return;
            }

            var userId = Guid.Parse(userIdStr);
            var belongs = await _cohortService.UserBelongsToCohortAsync(userId, cohortId, Context.ConnectionAborted);
            if (!belongs)
            {
                _logger.LogWarning("User {UserId} denied joining cohort {CohortId}", userId, cohortId);
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"cohort:{cohortId}", Context.ConnectionAborted);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var http = Context.GetHttpContext();
            var cohortIdRaw = http?.Request.Query["cohortId"].ToString();
            if (Guid.TryParse(cohortIdRaw, out var cohortId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"cohort:{cohortId}", Context.ConnectionAborted);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}