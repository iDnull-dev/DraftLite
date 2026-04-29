using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DraftLite.Api.Hubs;

[Authorize]
public sealed class CollaborationHub : Hub
{
    public async Task JoinProject(string projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ProjectGroup(projectId));
        await Clients.Group(ProjectGroup(projectId)).SendAsync("UserPresenceChanged", new
        {
            user = Context.UserIdentifier ?? Context.ConnectionId,
            status = "joined"
        });
    }

    public async Task LeaveProject(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ProjectGroup(projectId));
        await Clients.Group(ProjectGroup(projectId)).SendAsync("UserPresenceChanged", new
        {
            user = Context.UserIdentifier ?? Context.ConnectionId,
            status = "left"
        });
    }

    public async Task BroadcastPageChange(string projectId, object payload)
    {
        await Clients.OthersInGroup(ProjectGroup(projectId)).SendAsync("PageContentChanged", payload);
    }

    public async Task BroadcastCursorMove(string projectId, object payload)
    {
        await Clients.OthersInGroup(ProjectGroup(projectId)).SendAsync("CursorMoved", payload);
    }

    private static string ProjectGroup(string projectId) => $"project:{projectId}";
}

