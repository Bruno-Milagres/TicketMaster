using MediatR;
using Microsoft.AspNetCore.SignalR;
using TicketMaster.Application.Notifications;
using TicketMaster.Web.Hubs;

namespace TicketMaster.Web.EventHandlers;

public sealed class AssentoLiberadoEventHandler : INotificationHandler<AssentoLiberadoNotification>
{
    private readonly IHubContext<TicketHub> _hubContext;
    private readonly ILogger<AssentoLiberadoEventHandler> _logger;

    public AssentoLiberadoEventHandler(IHubContext<TicketHub> hubContext, ILogger<AssentoLiberadoEventHandler> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Handle(AssentoLiberadoNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Assento {Assento} liberado (reserva expirada) no evento {EventId}",
            notification.AssentoCodigo, notification.EventId);

        await _hubContext.Clients
            .Group(notification.EventId.ToString())
            .SendAsync("AtualizarAssento", notification.AssentoCodigo, "Livre", notification.UsuarioId?.ToString() ?? "", cancellationToken);
    }
}
