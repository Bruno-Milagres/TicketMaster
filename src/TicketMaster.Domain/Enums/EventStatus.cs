namespace TicketMaster.Domain.Enums;

/// <summary>
/// Representa o ciclo de vida de um evento.
/// Rascunho → Publicado → Cancelado
/// </summary>
public enum EventStatus
{
    /// <summary>Evento em edição, ainda não visível ao público.</summary>
    Rascunho = 0,

    /// <summary>Evento publicado e disponível para reserva de ingressos.</summary>
    Publicado = 1,

    /// <summary>Evento cancelado. Ingressos não podem mais ser reservados.</summary>
    Cancelado = 2
}
