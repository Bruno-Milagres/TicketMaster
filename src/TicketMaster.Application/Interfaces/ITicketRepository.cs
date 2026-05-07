using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> ObterPorAssentoAsync(string assentoCodigo, Guid eventId);
    Task<IEnumerable<Ticket>> ObterPorEventoAsync(Guid eventId);
    Task<IEnumerable<Ticket>> ObterReservasVencidasAsync();
    Task AtualizarAsync(Ticket ticket);
    Task<IEnumerable<Ticket>> ObterTodosAsync();
}