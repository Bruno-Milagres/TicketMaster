using MediatR;
using TicketMaster.Domain.Events;

namespace TicketMaster.Application.Notifications;

public class PedidoConfirmadoNotification : INotification
{
    public PedidoConfirmadoEvent Event { get; }
    public PedidoConfirmadoNotification(PedidoConfirmadoEvent evento) => Event = evento;
}
