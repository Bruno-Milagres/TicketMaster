using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using TicketMaster.Application.Notifications;
using TicketMaster.Web.Hubs;

namespace TicketMaster.Web.EventHandlers;

public sealed class ReservaCanceladaEventHandler : INotificationHandler<ReservaCanceladaNotification>
{
    private readonly IHubContext<TicketHub> _hubContext;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ReservaCanceladaEventHandler> _logger;

    public ReservaCanceladaEventHandler(IHubContext<TicketHub> hubContext, IDistributedCache cache, ILogger<ReservaCanceladaEventHandler> logger)
    {
        _hubContext = hubContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task Handle(ReservaCanceladaNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Reserva do assento {Assento} cancelada no evento {EventId}",
            notification.AssentoCodigo, notification.EventId);

        await _hubContext.Clients
            .Group(notification.EventId.ToString())
            .SendAsync("AtualizarAssento", notification.AssentoCodigo, "Disponivel", cancellationToken);

        await _cache.RemoveAsync($"tickets:evento:{notification.EventId}", cancellationToken);
    }
}
