namespace TicketMaster.Domain.Events;

/// <summary>
/// Ocorre quando um usuário cancela sua própria reserva.
/// </summary>
public sealed record ReservaCanceladaEvent(
    Guid EventId,
    string AssentoCodigo,
    Guid UsuarioId
);
