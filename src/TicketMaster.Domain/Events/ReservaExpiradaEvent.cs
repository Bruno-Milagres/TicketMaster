namespace TicketMaster.Domain.Events;

/// <summary>
/// Ocorre quando uma reserva expira e o assento é liberado.
/// </summary>
public sealed record ReservaExpiradaEvent(
    Guid EventId,
    string AssentoCodigo
);
