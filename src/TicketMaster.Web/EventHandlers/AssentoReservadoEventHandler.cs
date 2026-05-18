using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using TicketMaster.Application.Notifications;
using TicketMaster.Web.Hubs;

namespace TicketMaster.Web.EventHandlers;

public sealed class AssentoReservadoEventHandler : INotificationHandler<AssentoReservadoNotification>
{
    private readonly IHubContext<TicketHub> _hubContext;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AssentoReservadoEventHandler> _logger;

    public AssentoReservadoEventHandler(IHubContext<TicketHub> hubContext, IDistributedCache cache, ILogger<AssentoReservadoEventHandler> logger)
    {
        _hubContext = hubContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task Handle(AssentoReservadoNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Assento {Assento} reservado no evento {EventId}",
            notification.AssentoCodigo, notification.EventId);

        var userIdStr = notification.UsuarioId?.ToString() ?? "";
        await _hubContext.Clients
            .Group(notification.EventId.ToString())
            .SendAsync("AtualizarAssento", notification.AssentoCodigo, "Reservado", userIdStr, cancellationToken);

        await _cache.RemoveAsync($"tickets:evento:{notification.EventId}", cancellationToken);
    }
}
