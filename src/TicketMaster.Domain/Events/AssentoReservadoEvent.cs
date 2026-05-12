namespace TicketMaster.Domain.Events;

/// <summary>
/// Ocorre quando um assento é reservado por um usuário.
/// </summary>
public sealed record AssentoReservadoEvent(
    Guid EventId,
    string AssentoCodigo,
    Guid UsuarioId
);
