using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Interfaces;

//==============================================================================================================
/// Contrato de acesso a dados para a entidade <see cref="Ticket"/>.
//==============================================================================================================
public interface ITicketRepository
{
    //Obtém um ingresso pelo código do assento e pelo evento
    Task<Ticket?> ObterPorAssentoAsync(string assentoCodigo, Guid eventId, CancellationToken cancellationToken = default);

    //Retorna todos os ingressos de um determinado evento
    Task<IEnumerable<Ticket>> ObterPorEventoAsync(Guid eventId, CancellationToken cancellationToken = default);

    //Retorna todos os ingressos com reservas já expiradas
    Task<IEnumerable<Ticket>> ObterReservasVencidasAsync(CancellationToken cancellationToken = default);

    //Persiste as alterações de um ingresso no banco de dados
    Task AtualizarAsync(Ticket ticket, CancellationToken cancellationToken = default);

    //Retorna todos os ingressos cadastrados
    Task<IEnumerable<Ticket>> ObterTodosAsync(CancellationToken cancellationToken = default);
}