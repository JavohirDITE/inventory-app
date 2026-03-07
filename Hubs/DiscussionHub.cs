using Microsoft.AspNetCore.SignalR;

namespace InventoryApp.Hubs;

public class DiscussionHub : Hub
{
    public async Task JoinGroup(string inventoryId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, inventoryId);
    }

    public async Task LeaveGroup(string inventoryId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, inventoryId);
    }
}
