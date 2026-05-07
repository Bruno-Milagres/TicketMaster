using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Interfaces;

/// <summary>
/// Contrato de acesso a dados para a entidade <see cref="Ticket"/>.
/// </summary>
public interface ITicketRepository
{
    /// <summary>Obtém um ingresso pelo código do assento e pelo evento.</summary>
    Task<Ticket?> ObterPorAssentoAsync(string assentoCodigo, Guid eventId);

    /// <summary>Retorna todos os ingressos de um determinado evento.</summary>
    Task<IEnumerable<Ticket>> ObterPorEventoAsync(Guid eventId);

    /// <summary>Retorna todos os ingressos com reservas já expiradas.</summary>
    Task<IEnumerable<Ticket>> ObterReservasVencidasAsync();

    /// <summary>Persiste as alterações de um ingresso no banco de dados.</summary>
    Task AtualizarAsync(Ticket ticket);

    /// <summary>Retorna todos os ingressos cadastrados.</summary>
    Task<IEnumerable<Ticket>> ObterTodosAsync();
}