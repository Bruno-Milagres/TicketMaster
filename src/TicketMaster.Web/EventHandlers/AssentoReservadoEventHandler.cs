using MediatR;
using Microsoft.AspNetCore.SignalR;
using TicketMaster.Application.Notifications;
using TicketMaster.Web.Hubs;

namespace TicketMaster.Web.EventHandlers;

public sealed class AssentoReservadoEventHandler : INotificationHandler<AssentoReservadoNotification>
{
    private readonly IHubContext<TicketHub> _hubContext;
    private readonly ILogger<AssentoReservadoEventHandler> _logger;

    public AssentoReservadoEventHandler(IHubContext<TicketHub> hubContext, ILogger<AssentoReservadoEventHandler> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Handle(AssentoReservadoNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Assento {Assento} reservado no evento {EventId}",
            notification.AssentoCodigo, notification.EventId);

        await _hubContext.Clients
            .Group(notification.EventId.ToString())
            .SendAsync("AtualizarAssento", notification.AssentoCodigo, "Reservado", cancellationToken);
    }
}
