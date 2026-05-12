using MediatR;

namespace TicketMaster.Application.Notifications;

public sealed record ReservaCanceladaNotification(
    Guid EventId,
    string AssentoCodigo,
    string Status
) : INotification;
