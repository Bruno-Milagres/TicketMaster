using MediatR;
using TicketMaster.Application.Interfaces;

namespace TicketMaster.Application.Notifications;

public class PedidoConfirmadoNotificationHandler : INotificationHandler<PedidoConfirmadoNotification>
{
    private readonly IEmailService _emailService;

    public PedidoConfirmadoNotificationHandler(IEmailService emailService) => _emailService = emailService;

    public async Task Handle(PedidoConfirmadoNotification notification, CancellationToken cancellationToken)
    {
        var e = notification.Event;
        if (string.IsNullOrWhiteSpace(e.ClienteEmail)) return;

        await _emailService.SendOrderConfirmationAsync(e.ClienteEmail, e.ClienteNome ?? "Cliente", e.Pedido);
    }
}
