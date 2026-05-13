using MediatR;

namespace TicketMaster.Application.Notifications;

public sealed record AssentoLiberadoNotification(
    Guid EventId,
    string AssentoCodigo,
    string Status
) : INotification;
