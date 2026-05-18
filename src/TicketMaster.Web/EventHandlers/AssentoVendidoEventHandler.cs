using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using TicketMaster.Application.Notifications;
using TicketMaster.Web.Hubs;

namespace TicketMaster.Web.EventHandlers;

public sealed class AssentoVendidoEventHandler : INotificationHandler<AssentoVendidoNotification>
{
    private readonly IHubContext<TicketHub> _hubContext;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AssentoVendidoEventHandler> _logger;

    public AssentoVendidoEventHandler(IHubContext<TicketHub> hubContext, IDistributedCache cache, ILogger<AssentoVendidoEventHandler> logger)
    {
        _hubContext = hubContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task Handle(AssentoVendidoNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Assento {Assento} vendido no evento {EventId}",
            notification.AssentoCodigo, notification.EventId);

        await _hubContext.Clients
            .Group(notification.EventId.ToString())
            .SendAsync("AtualizarAssento", notification.AssentoCodigo, "Vendido", notification.UsuarioId?.ToString() ?? "", cancellationToken);

        await _cache.RemoveAsync($"tickets:evento:{notification.EventId}", cancellationToken);
    }
}
