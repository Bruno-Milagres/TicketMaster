using MediatR;

namespace TicketMaster.Application.Notifications;

public sealed record AssentoVendidoNotification(
    Guid EventId,
    string AssentoCodigo,
    string Status,
    Guid? UsuarioId = null
) : INotification;
