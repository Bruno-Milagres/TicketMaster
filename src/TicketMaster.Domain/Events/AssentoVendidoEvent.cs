namespace TicketMaster.Domain.Events;

/// <summary>
/// Ocorre quando o pagamento de um ingresso é confirmado.
/// </summary>
public sealed record AssentoVendidoEvent(
    Guid EventId,
    string AssentoCodigo,
    Guid UsuarioId
);
