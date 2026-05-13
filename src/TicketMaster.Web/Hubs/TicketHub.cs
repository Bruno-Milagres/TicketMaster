using Microsoft.AspNetCore.SignalR;

namespace TicketMaster.Web.Hubs;

public class TicketHub : Hub
{
    //========================================================================================================
    // Método chamado pelos clientes para entrar no grupo do evento específico
    //========================================================================================================
    public async Task EntrarNaSalaDoEvento(string eventId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, eventId);
    }
}