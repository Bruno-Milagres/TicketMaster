using MediatR;

namespace TicketMaster.Application.Notifications;

public sealed record AssentoReservadoNotification(
    Guid EventId,
    string AssentoCodigo,
    string Status
) : INotification;
