using Microsoft.AspNetCore.SignalR;

namespace TicketMaster.Web.Hubs;

public class TicketHub : Hub
{
    public async Task EntrarNaSalaDoEvento(string eventId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, eventId);
    }
}