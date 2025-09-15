using AlgoDuck.Modules.Cohort.DTOs;
using AlgoDuck.Modules.Cohort.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AlgoDuck.Modules.Cohort;

[Authorize]
public class CohortChatHub : Hub
{
    private readonly ICohortChatService _chatService;

    public CohortChatHub(ICohortChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task SendMessage(CohortChatDto dto)
    {
        var broadcast = await _chatService.SaveMessageAsync(dto);
        await Clients.Group(dto.CohortId.ToString()).SendAsync("ReceiveMessage", broadcast);
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var cohortId = httpContext?.Request.Query["cohortId"].ToString();

        if (Guid.TryParse(cohortId, out var groupId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var httpContext = Context.GetHttpContext();
        var cohortId = httpContext?.Request.Query["cohortId"].ToString();

        if (Guid.TryParse(cohortId, out var groupId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
        }

        await base.OnDisconnectedAsync(exception);
    }
}